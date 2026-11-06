# Linux Permission Helper

A native C#/.NET 10 WinForms utility for understanding, converting and generating standard Linux `chmod` permissions. It uses the Engineer-Forge Dark-Mode UI Template v1 and preserves the behaviour of the original PowerShell/WPF Linux Permission Helper, which remains the functional reference and is not modified by this project.

| | |
| --- | --- |
| Version | 1.0.0 |
| Author | Sean Moreira (Engineer-Forge) |
| Platform | Windows 10 (1809) or later, x64 |
| Runtime | Bundled. No .NET installation is required |
| Artefact | `Linux-Permission-Helper.exe`, self-contained single file, ~113 MiB |

## License

Copyright (c) 2026 Sean Moreira. Licensed under the [MIT License](LICENSE).

The reusable DarkModeUI library is separately MIT-licensed in `../Dark-Mode-UI-Template/LICENSE`. The original Engineer-Forge artwork and icon are offered under MIT to the extent the copyright holder has rights to license them. See `docs/ARTWORK-NOTICE.md` and `docs/LICENSING-STATUS.md`. Microsoft .NET runtime components retain their own third-party licenses and notices.

The Windows executable is intentionally unsigned by owner decision (2026-11-04). Windows may display SmartScreen or Unknown Publisher warnings. Compare the download's SHA-256 with the hash published through a trusted channel; checksums verify integrity, not publisher identity. The public v1.0.0 release was authorized by the copyright holder.

## Functional scope

- Owner, Group and Others read/write/execute permission controls
- Exact three-digit octal modes from `000` through `777`
- Nine-character symbolic `rwx` conversion
- Human-readable permission explanations and access meanings
- Named profiles and Low/Moderate/High risk classification
- Presets `600`, `644`, `700`, `750`, `755` and `775`
- Standard `ls -l` permission parsing for files, directories, links, devices, pipes and sockets
- Safely quoted copy-ready `chmod` commands, including recursive mode
- Clipboard copy without PowerShell

The application never executes the generated command and never connects to a Linux
system. It produces text for the operator to review and run.

V1 intentionally rejects setuid, setgid and sticky-bit fields (`s`, `S`, `t`, `T`).

## Generated command safety

The generated command emits the target path as a single literal POSIX shell argument:

- The path is single-quoted whenever it contains a character outside the shell-safe
  set, so spaces, quotes, apostrophes, semicolons, dollar signs, command
  substitutions, wildcards and control characters cannot be reinterpreted by the
  shell. An apostrophe inside the path is emitted with the standard `'\''` sequence.
- A path that starts with a dash is preceded by the `--` option boundary so it can
  never be parsed as a `chmod` option. A path of exactly `-` is emitted as `./-`,
  because a bare `-` is the conventional stand-in for standard input.
- The path is trimmed before use, so surrounding whitespace typed by the operator
  cannot change the generated argument.

The quoting contract is verified two ways: by a POSIX shell tokenizer that must
reproduce the typed path exactly, and by executing the generated commands against
throwaway files in a real Linux shell.

## Project structure

```text
src/LinuxPermissionHelper.Core/     Permission models and independently testable services
src/LinuxPermissionHelper.App/      DarkModeUI-based WinForms application
tests/LinuxPermissionHelper.Tests/  Dependency-free functional regression runner
Assets/Icons/                       Approved source artwork and generated Windows icon
artifacts/dpi-snapshots/            Generated 100%, 125% and 150% visual QA renders
publish/                            Self-contained single-file executable
```

## DarkModeUI relationship

The WinForms project references the canonical reusable library at:

```text
..\Dark-Mode-UI-Template\src\DarkModeUI\DarkModeUI.csproj
```

No independent theme or duplicate palette is maintained in this application.
Application code uses `DarkForm`, reusable dark controls and the centralized
template tokens.

## Icon

The approved artwork supplied as `Assets\Icons\ICON.ico` contains PNG data despite
its legacy extension. The original file is preserved unchanged.
`Assets\Icons\Linux-Permission-Helper.ico` is the generated Windows icon container
used by the executable and includes 16, 20, 24, 32, 40, 48, 64, 128 and 256 pixel
images.

## Build, test and publish

Run from PowerShell:

```powershell
.\build.ps1
```

The script copies the project and the canonical DarkModeUI library into a unique
local temporary directory, purges any pre-existing build intermediates from that
copy, runs the regression suite, builds Release, publishes a self-contained
single-file `win-x64` executable, returns the executable to `publish`, generates DPI
snapshots, and removes the temporary files.

To skip snapshot regeneration:

```powershell
.\build.ps1 -SkipSnapshots
```

### Reproducible publishing

The published executable is **byte-for-byte reproducible from any build directory**
when it is produced by the documented wrapper. `build.ps1` enables the
continuous-integration build mode and maps the staged source root to a fixed
virtual root, which removes the build-location dependency described in
`docs\REPRODUCIBLE-BUILDS.md`. The script prints the SHA-256 of the artefact it
produced.

The hash is a function of the source content, the source revision and the build
procedure. The wrapper obtains Git HEAD before staging and passes it explicitly to
MSBuild as `SourceRevisionId`; release builds therefore require a committed Git
checkout and include that revision in ProductVersion. **Always release with `.\build.ps1`**: a publish performed directly in
the source tree produces a different artefact, and the hash in a release manifest
belongs to the wrapper.

Two switches exist for unusual environments:

- `-DisablePathVirtualization` stops the wrapper from mapping the staged tree to a
  short drive letter for the regression-test launch. The published artefact is
  unaffected.
- `-AllowNonReproducibleBuild` disables the path mapping. The artefact then changes
  bytes with the build directory and the script says so.

Run only the core regression suite with:

```powershell
dotnet run --project .\tests\LinuxPermissionHelper.Tests\LinuxPermissionHelper.Tests.csproj -c Release
```

The published executable requires neither PowerShell nor an externally installed
.NET runtime.

## Documentation

| Document | Purpose |
| --- | --- |
| `CHANGELOG.md` | Release history |
| `docs\LICENSING-STATUS.md` | Licence, artwork rights and redistribution status |
| `docs\REPRODUCIBLE-BUILDS.md` | Why and how the artefact hash is reproducible |
| `docs\VERIFICATION.md` | What was verified for this release and what was not |
| `docs\THIRD-PARTY-NOTICES.txt` | Notices for the bundled .NET runtime |

## Security and privacy

The application is offline. It makes no network connection, starts no child
process, reads no environment variable and stores no telemetry. The only value it
writes outside your control is the permission mode you type and the path you type,
both of which stay in the window unless you copy the command to the clipboard.
