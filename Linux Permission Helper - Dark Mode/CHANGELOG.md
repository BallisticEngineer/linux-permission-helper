# Changelog

All notable changes to Linux Permission Helper are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses
[semantic versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-10-31

First release-qualified version of the .NET 10 WinForms application. It supersedes
the historical PowerShell/WPF helper, which remains the functional reference.

### Added

- Owner, Group and Others permission controls with a linked octal editor.
- Nine-character symbolic `rwx` conversion and human-readable explanations.
- Named permission profiles and Low/Moderate/High risk classification.
- Presets `600`, `644`, `700`, `750`, `755` and `775`.
- Standard `ls -l` parsing for files, directories, links, devices, pipes and sockets.
- Copy-ready `chmod` command generation, with optional recursive mode.
- Windows clipboard copy that does not require PowerShell.
- A dependency-free console regression runner with 918 cases.

### Changed

- Generated commands now emit the path as a single literal POSIX shell argument.
  Single quoting is applied to every character outside the shell-safe set, an
  apostrophe is escaped with `'\''`, and a `--` option boundary is emitted for a
  path that starts with a dash. A path of exactly `-` is emitted as `./-`.
- Octal and symbolic validation is now whole-string. A trailing newline or any
  other control character is rejected instead of being silently accepted.
- An invalid octal mode now presents a single consistent state: the symbolic
  output, the generated command, the effective summary and the permission digits
  are cleared, the permission checkboxes are cleared and disabled, and the header
  and risk chips show `INVALID`. Entering a valid mode restores everything.
- Every interactive control carries a stable `Control.Name` and a meaningful
  accessible name, including the inner edit of each composite text field, so the
  application can be driven by UI Automation.
- The build wrapper no longer copies pre-existing `bin` and `obj` directories into
  the staging tree, so a staged build can never reuse artefacts produced elsewhere.
- The build wrapper publishes reproducibly: the artefact hash no longer depends on
  the directory the project is built from.

### Known limitations

- The Visual Studio designer surface has not been loaded and is therefore
  unverified. The form builds its interface in code rather than through
  `InitializeComponent`.
- Real 150% and 200% monitor scaling, high contrast and screen-reader behaviour
  have not been exercised; all development displays ran at 96 DPI.
- The executable is not code signed.
- The licence status of the source and the icon artwork is unresolved. See
  `docs\LICENSING-STATUS.md`.

[1.0.0]: https://example.invalid/linux-permission-helper/releases/1.0.0
