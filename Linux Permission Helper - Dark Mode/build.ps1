[CmdletBinding()]
param(
    [switch]$SkipSnapshots,
    [switch]$DisablePathVirtualization,
    [switch]$AllowNonReproducibleBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectRoot = $PSScriptRoot
# The staged source has no .git directory. Resolve the source revision before staging
# and pass it explicitly so ProductVersion always identifies the committed source.
$sourceRevision = (& git -C $projectRoot rev-parse HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or $sourceRevision -notmatch '^[0-9a-fA-F]{40}$') {
    throw 'A committed Git source revision is required for a traceable release build.'
}
$sourceRevision = $sourceRevision.Trim()
$toolsRoot = Split-Path -Parent $projectRoot
$templateRoot = Join-Path $toolsRoot 'Dark-Mode-UI-Template'
$templateLibrary = Join-Path $templateRoot 'src\DarkModeUI'
$publishDirectory = Join-Path $projectRoot 'publish'
$snapshotDirectory = Join-Path $projectRoot 'artifacts\dpi-snapshots'

# The staging identifier is kept short on purpose. The absolute path of the staged
# regression test executable is what previously exceeded the Windows process launch
# limit, so every avoidable character is removed from the staging root.
$buildId = [Guid]::NewGuid().ToString('N').Substring(0, 8)
$stagingRoot = Join-Path ([System.IO.Path]::GetTempPath()) "lph-$buildId"
$stagingProject = Join-Path $stagingRoot 'Linux Permission Helper - Dark Mode'
$stagingTemplate = Join-Path $stagingRoot 'Dark-Mode-UI-Template\src\DarkModeUI'
$stagingPublish = Join-Path $stagingRoot 'publish'

if (-not (Test-Path -LiteralPath $templateLibrary -PathType Container)) {
    throw "DarkModeUI library was not found at $templateLibrary."
}

function Find-FreeDriveLetter {
    foreach ($candidate in 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z') {
        $root = "$candidate`:"
        if (-not (Test-Path -LiteralPath $root)) {
            return $candidate
        }
    }

    return $null
}

# Launch path virtualisation.
#
# The staging tree is also reachable through a short drive letter. That alias is
# used only to start the staged regression test executable, because a process
# launch is the one operation that fails outright when the absolute path is too
# long. Every build and publish step deliberately uses the real staging path so
# that the compiler always sees the same physical source location and the
# published artefact stays reproducible.
New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
$mappedDrive = $null
if (-not $DisablePathVirtualization) {
    $letter = Find-FreeDriveLetter
    if ($null -ne $letter) {
        try {
            & subst "$letter`:" $stagingRoot | Out-Null
        }
        catch {
            Write-Warning "Launch path virtualisation was not available: $($_.Exception.Message)"
        }

        if ($LASTEXITCODE -eq 0 -and (Test-Path -LiteralPath "$letter`:")) {
            $mappedDrive = $letter
        }
    }
}

# Guard the documented failure mode with an actionable message instead of the
# opaque "The filename or extension is too long" process launch failure.
$testProjectRelative = 'tests\LinuxPermissionHelper.Tests\LinuxPermissionHelper.Tests.csproj'
$testExecutableRelative = 'tests\LinuxPermissionHelper.Tests\bin\Release\net10.0\LinuxPermissionHelper.Tests.exe'
$realTestExecutable = Join-Path $stagingProject $testExecutableRelative
if ($mappedDrive) {
    $launchTestExecutable = Join-Path "$mappedDrive`:\" ("Linux Permission Helper - Dark Mode\$testExecutableRelative")
}
else {
    $launchTestExecutable = $realTestExecutable
}

if ($launchTestExecutable.Length -ge 240) {
    throw "The staged regression test executable path is $($launchTestExecutable.Length) characters long and cannot be launched. Re-run with a shorter TEMP directory, or leave launch path virtualisation enabled."
}

try {
    New-Item -ItemType Directory -Path $stagingProject -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Parent $stagingTemplate) -Force | Out-Null

    Copy-Item -LiteralPath (Join-Path $projectRoot 'src') -Destination (Join-Path $stagingProject 'src') -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $projectRoot 'tests') -Destination (Join-Path $stagingProject 'tests') -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $projectRoot 'Assets') -Destination (Join-Path $stagingProject 'Assets') -Recurse -Force
    Copy-Item -LiteralPath $templateLibrary -Destination $stagingTemplate -Recurse -Force

    # Build intermediates must never travel into the staging tree. A copied bin or
    # obj directory lets the incremental build reuse artefacts produced at a different
    # source location, which silently changes the published bytes and breaks the
    # isolation the staging step exists to provide.
    Get-ChildItem -LiteralPath $stagingRoot -Recurse -Directory -Force |
        Where-Object { $_.Name -in @('bin', 'obj') } |
        ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force }

    $appProject = Join-Path $stagingProject 'src\LinuxPermissionHelper.App\LinuxPermissionHelper.App.csproj'
    $testProject = Join-Path $stagingProject $testProjectRelative

    dotnet restore $appProject --disable-parallel
    dotnet restore $testProject --disable-parallel

    # Reproducible compiling.
    #
    # The .NET [GeneratedRegex] source generator embeds a SHA-256 fingerprint of its
    # inputs in the name of the generated implementation type. That fingerprint
    # includes the source location, so the compiled Core assembly - and therefore the
    # single-file bundle that embeds it - changes bytes whenever the project is built
    # from a different directory. Mapping the staged source root to a fixed virtual
    # root and enabling the continuous-integration build mode removes that dependency,
    # so the published executable is byte-for-byte identical from any build location.
    #
    # The mapping must be applied to every compile, not only to the publish. A build
    # step without the mapping leaves an already compiled Core assembly in place that
    # the publish then bundles unchanged, which silently reintroduces the dependency.
    $reproducibleArguments = @(
        "-p:SourceRevisionId=$sourceRevision",
        '-p:IncludeSourceRevisionInInformationalVersion=true'
    )
    if (-not $AllowNonReproducibleBuild) {
        $reproducibleArguments += '-p:ContinuousIntegrationBuild=true'
        $reproducibleArguments += "-p:PathMap=$stagingRoot=/_/"
    }

    dotnet build $testProject -c Release --no-restore --disable-build-servers --no-incremental -m:1 @reproducibleArguments
    if ($LASTEXITCODE -ne 0) {
        throw 'Regression test build failed.'
    }

    & $launchTestExecutable
    if ($LASTEXITCODE -ne 0) {
        throw 'Functional regression tests failed.'
    }

    dotnet build $appProject -c Release --no-restore --disable-build-servers --no-incremental -m:1 @reproducibleArguments

    $publishArguments = @(
        'publish', $appProject,
        '-c', 'Release',
        '-r', 'win-x64',
        '--self-contained', 'true',
        '--no-restore',
        '--disable-build-servers',
        '-m:1',
        '-o', $stagingPublish,
        '-p:PublishSingleFile=true',
        '-p:DebugType=None',
        '-p:DebugSymbols=false'
    ) + $reproducibleArguments

    dotnet @publishArguments
    if ($LASTEXITCODE -ne 0) {
        throw 'Publish failed.'
    }

    $stagedFiles = @(Get-ChildItem -LiteralPath $stagingPublish -File)
    if ($stagedFiles.Count -ne 1 -or $stagedFiles[0].Name -ne 'Linux-Permission-Helper.exe') {
        throw "Expected one Linux-Permission-Helper.exe artifact in $stagingPublish."
    }

    New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null
    Get-ChildItem -LiteralPath $publishDirectory -File -Force |
        Where-Object { -not ($_.Attributes -band [IO.FileAttributes]::Hidden) } |
        ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force }
    Copy-Item -LiteralPath $stagedFiles[0].FullName -Destination $publishDirectory -Force
    $artifact = Get-Item -LiteralPath (Join-Path $publishDirectory 'Linux-Permission-Helper.exe')

    if (-not $SkipSnapshots) {
        New-Item -ItemType Directory -Path $snapshotDirectory -Force | Out-Null
        Get-ChildItem -LiteralPath $snapshotDirectory -Filter 'linux-permission-helper-*.png' -File -Force |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -Force }
        & $artifact.FullName --snapshot-dir $snapshotDirectory
        $snapshots = @()
        for ($attempt = 0; $attempt -lt 20 -and $snapshots.Count -ne 3; $attempt++) {
            Start-Sleep -Milliseconds 250
            $snapshots = @(Get-ChildItem -LiteralPath $snapshotDirectory -Filter 'linux-permission-helper-*.png' -File)
        }
        if ($snapshots.Count -ne 3) {
            throw "Expected three DPI snapshots in $snapshotDirectory."
        }
    }

    Write-Host "Published: $($artifact.FullName)"
    Write-Host "Size: $([Math]::Round($artifact.Length / 1MB, 2)) MiB"
    Write-Host "SHA-256: $((Get-FileHash -LiteralPath $artifact.FullName -Algorithm SHA256).Hash)"
    Write-Host "Staging root: $stagingRoot"
    if ($mappedDrive) {
        Write-Host "Launch path virtualisation: regression test executable launched through $mappedDrive`:"
    }
    if ($AllowNonReproducibleBuild) {
        Write-Host "Reproducible publishing: DISABLED (the artefact hash depends on the build directory)"
    }
    else {
        Write-Host "Reproducible publishing: enabled (continuous integration build with a fixed virtual source root)"
    }
    if (-not $SkipSnapshots) {
        Write-Host "DPI snapshots: $snapshotDirectory"
    }
}
finally {
    if ($mappedDrive) {
        & subst "$mappedDrive`:" /D | Out-Null
    }

    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }
}
