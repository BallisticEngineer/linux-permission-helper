# Reproducible builds and the executable hash

This note explains why the published executable hash used to change on every build,
what actually caused it, and how the current build removes the dependency.

## The symptom

The same source produced different `Linux-Permission-Helper.exe` hashes depending
on where it was built:

| Build | Hash |
| --- | --- |
| Wrapper build in the Stage 03B workspace | `9573B85E…6DB40` |
| Direct `dotnet publish` in the same workspace | `7EA4D46B…28FF5` |
| Direct publish from a different directory | `60E1BE08…0F65D`, `76C294FC…31CB21`, `AF6AB483…AD53E5` |

Two publishes from the *same* directory always produced the *same* hash, which
ruled out timestamps and toolchain non-determinism and pointed at the build
location.

## The cause

The .NET `[GeneratedRegex]` source generator emits a generated implementation type
whose name contains a SHA-256 fingerprint of the generator's inputs:

```text
<RegexGenerator_g>F35C73C63341A2F5058C323488DEA76E3CF7EBE36B6AD25E8093B6AEB153B2FE2__RequiresQuoting_0
```

That fingerprint depends on the source tree location. The name is part of the
assembly metadata, so the compiled `EngineerForge.LinuxPermissionHelper.Core.dll`
changes whenever the project is built from a different directory, and the
single-file bundle that embeds it changes with it.

The difference was located exactly. Comparing two otherwise identical bundles
byte-for-byte gave **332 differing bytes in 28 regions**, all inside the embedded
Core assembly, and every differing region was one of those fingerprint strings.
The application assembly, the apphost and `deps.json` were byte-identical across
all builds.

## The fix

The .NET compiler understands `/pathmap`, which rewrites a source prefix to a
fixed virtual prefix before the compilation sees it. Combined with the
continuous-integration build mode, the build becomes independent of where the source
lives:

```text
-p:ContinuousIntegrationBuild=true
-p:PathMap=<staged source root>=/_/
```

`build.ps1` applies both automatically. The result is that the published
executable is byte-for-byte identical no matter which directory the project is
built from, which is what makes the release hash meaningful.

## What the hash is a function of

The artefact hash is a function of three things: the **source content**, the
**source revision** and the **build procedure**. Two consequences follow.

- **The build procedure is part of the identity.** Always release with
  `.\build.ps1`. A `dotnet publish` performed directly in the source tree, without
  the wrapper's staged copy, produces a different — and internally consistent —
  artefact: the difference is confined to the compiled assemblies inside the
  bundle and the path mapping does not normalise it completely. The hash recorded
  in a release manifest belongs to the documented wrapper and to nothing else.
- **The source revision is part of the identity.** The product version carries the
  commit, so the hash belongs to a commit rather than to a file tree. Any later
  source change produces a new commit and therefore a new hash, by design.

## A second defect found at the same time

The wrapper copied the whole `src`, `tests` and `Assets` trees into the staging
directory, including any `bin` and `obj` directories that happened to exist in the
working copy. A copied intermediate directory lets the incremental build reuse
artefacts produced at a different source location, which silently changes the
published bytes and defeats the isolation the staging step exists to provide. The
wrapper now purges `bin` and `obj` from the staged tree before building.

## A note on the drive-letter alias

The wrapper maps the staged tree to a short drive letter so that the staged
regression-test executable can be launched even when its real path is too long for
Windows. That alias is used **only** for the launch. Builds and publishes always
use the real path: when the compiler is given `L:\…` paths, the path mapping
produces a different virtual prefix and therefore different bytes.

## Verifying the result

```powershell
.\build.ps1 -SkipSnapshots
```

The script prints the SHA-256 of the artefact it produced. Running it repeatedly,
from different directories, or with `-DisablePathVirtualization`, produces the same
hash. `-AllowNonReproducibleBuild` restores the old behaviour and the script says
so in its output.
