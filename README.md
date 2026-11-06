# Linux Permission Helper

A free, offline Windows utility for understanding Linux file permissions and generating safely quoted chmod commands. Built in C# / .NET 10 with WinForms and the Engineer-Forge DarkModeUI library.

**Version:** 1.0.0 | **License:** MIT | **Platform:** Windows x64

## Download

**[Download Linux Permission Helper v1.0.0](https://github.com/BallisticEngineer/linux-permission-helper/releases/latest)**

Download Linux-Permission-Helper-v1.0.0.zip, extract it, and launch Linux-Permission-Helper.exe.

- Windows 10 (1809) or later, 64-bit.
- Portable, self-contained executable. No separate .NET installation needed.
- The application is **unsigned**. Windows may show Unknown Publisher or SmartScreen warnings. The accompanying .zip.sha256 lets you compare the ZIP bytes against a hash published through a trusted channel. A checksum is not publisher authentication.

## Features

- Convert three-digit octal Linux modes (000-777) and symbolic rwx permissions.
- Adjust owner, group, and others read/write/execute settings.
- Presets for 600, 644, 700, 750, 755, and 775.
- Interpret permissions, risk classifications, and standard ls -l strings.
- Generate copy-ready chmod commands, optionally recursive, with literal POSIX shell-safe path quoting.

**The program never executes a command or connects to a Linux host.** It only generates command text for an operator to review. Setuid, setgid and sticky-bit permission fields are intentionally outside the v1.0.0 scope.

## Source code

- [Linux Permission Helper application, tests, and build wrapper](Linux%20Permission%20Helper%20-%20Dark%20Mode/)
- [Reusable DarkModeUI library](Dark-Mode-UI-Template/)

The WinForms app references the bundled DarkModeUI library as a C# project reference.

## Build and test

Requirements: Windows, Git, .NET 10 SDK, and PowerShell 7.

From the application subdirectory run:

    ./build.ps1 -SkipSnapshots

The wrapper restores dependencies, runs the functional regression suite, builds Release, and writes a self-contained Windows x64 executable into the app's publish directory. It embeds the current Git source revision in the product version. See the [application README](Linux%20Permission%20Helper%20-%20Dark%20Mode/README.md) for details.

## Release provenance

The published v1.0.0 executable was built and qualified from a separate private engineering worktree at revision 696672e430d0207bb4d6ab03171369d74fd39444. This public repository contains a **curated, byte-identical snapshot of the application's and library's source files**, without that worktree's internal reports, experimental scripts, or private Git history.

The public v1.0.0 tag identifies the public source snapshot, not the original private revision. Consequently, a rebuild from this public Git revision embeds a different ProductVersion revision identifier and **will not yield the same executable SHA-256** as the distribution ZIP.

Release executable SHA-256: 8776871CE472AA92F8D57E1B28BD28AC3047FF729770A1CBC57E59AE43E4F806.

## Licensing

Copyright (c) 2026 Sean Moreira.

Linux Permission Helper and Engineer-Forge DarkModeUI are under [MIT License](LICENSE), with separate [DarkModeUI license](Dark-Mode-UI-Template/LICENSE). Engineer-Forge-owned artwork is included to the extent the copyright holder can grant those rights. See the [artwork notice](Linux%20Permission%20Helper%20-%20Dark%20Mode/docs/ARTWORK-NOTICE.md). Third-party .NET runtime licenses are distributed with the release.

## Validation

v1.0.0 passed functional regression and Windows UI Automation checks. Real-monitor 150%/200% scaling and exhaustive screen-reader/keyboard navigation were not separately tested. No Visual Studio Designer support is needed or claimed.
