# Changelog

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and the project follows [Semantic Versioning](https://semver.org/).

## [2.1.0] - 2026-09-22

### Changed
- The options page is now a table of rules (on/off, name, regular expression, ignore case, style)
  with Add, Remove, Move up/down and Reset to defaults buttons, an on/off checkbox, and a field to test a line of code.
- Settings are stored in a new format; rules customized in 2.0.0 are replaced by the defaults.

### Added
- Predefined rules: "Logger calls" (enabled), "Console output" and "Debug / Trace output" (disabled).

## [2.0.0] - 2026-09-22

Complete rewrite for Visual Studio 2026.

### Added
- Rules as regular expressions in *Tools > Options > GrayLog > General*, one per line, with comments.
- Three style slots (`GrayLog - Style 1..3`) configurable in *Fonts and Colors*.
- Multi-line calls are dimmed up to the closing parenthesis (limit: 30 lines).
- *Tools > Toggle GrayLog* command, default shortcut `Ctrl+Alt+Shift+G`; the state is persisted.
- Settings changes apply to open documents without restart.
- Unit tests for rule parsing and line matching.

### Removed
- Glyph margin marker.
- Support for Visual Studio 2017/2019 and 2022.

## Legacy versions

Versions 1.0–1.2 (2017–2018) and 2019.1–2019.5 (2019–2020) are the previous implementation
for Visual Studio 2017/2019. They are tagged `legacy/<version>`.

[2.1.0]: https://github.com/vdasus/GrayLogs/releases/tag/v2.1.0
[2.0.0]: https://github.com/vdasus/GrayLogs/releases/tag/v2.0.0
