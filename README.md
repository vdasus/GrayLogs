# GrayLog

Visual Studio 2026 extension that de-emphasizes lines matching configurable regular expressions.
By default it dims logging calls such as `log.Info(...)`, `_logger.LogInformation(...)` and `Logger?.Warn(...)`,
including multi-line calls up to the closing parenthesis.

> **Disclaimer.** This is a personal project, written for the author's own use. It is provided "as is",
> without warranty of any kind, and without support. Use it at your own risk.
> Version 2.x was developed with an AI coding assistant (Claude Code) under the supervision of
> [vdasus](https://github.com/vdasus).

## Installation

Download `GrayLog-<version>.vsix` from [Releases](https://github.com/vdasus/GrayLogs/releases),
close Visual Studio and open the file (or run `VSIXInstaller.exe GrayLog-<version>.vsix`).
Requires Visual Studio 2026 or later (amd64 or arm64).

## Usage

- **Rules:** *Tools > Options > GrayLog > General > Rules*. One regex per line; `#` starts a comment;
  an optional prefix `2: ` or `3: ` selects the style slot; `(?i)` makes a rule case-insensitive.
- **Colors:** *Tools > Options > Environment > Fonts and Colors*, items `GrayLog - Style 1..3`.
- **On/off:** *Tools > Toggle GrayLog* or `Ctrl+Alt+Shift+G`.

## Build

```
MSBuild.exe GrayLogs.slnx -restore -p:Configuration=Release
dotnet test --project tests/GrayLog.Tests
```

A Release build writes the package to `publish/GrayLog-<version>.vsix`.

## Versioning

The project follows [Semantic Versioning](https://semver.org/). The version is set in
`src/GrayLog/GrayLog.csproj` and `src/GrayLog/source.extension.vsixmanifest` (the build fails if they differ);
changes are listed in [CHANGELOG.md](CHANGELOG.md). Releases are tagged `vMAJOR.MINOR.PATCH`.
Versions 1.0–1.2 and 2019.1–2019.5 belong to the previous implementation (Visual Studio 2017/2019)
and are tagged `legacy/<version>`.

See [Docs/PLAN.md](Docs/PLAN.md) for design details.

## License

[MIT](LICENSE).
