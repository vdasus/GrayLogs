# GrayLog for Visual Studio 2026 — Implementation Plan

Status: implemented (see section 8); manual verification in Visual Studio pending.
Date: 2026-09-22.

## 1. Goal

A Visual Studio 2026 extension that visually de-emphasizes lines matching user-defined patterns
(for example, logging calls such as `_logger.LogInformation(...)`, `log.Debug(...)`, `Log?.Warn(...)`),
so that business logic stands out and logging recedes into the background.

Scope of v1:

- Visual Studio 2026 and later (product version 18.x+).
- Patterns are regular expressions configured in global Tools > Options.
- Each pattern uses one of three style slots; colors are edited in *Fonts and Colors*.
- The whole line is dimmed; a multi-line call is dimmed until its closing parenthesis.
- Quick on/off toggle via a menu command and a keyboard shortcut.
- Distribution: local `.vsix` only. Display name stays **GrayLog**.

## 2. Assessment of the existing code

The current repository (VS 2019, .NET Framework 4.8, `packages.config`, legacy `.csproj`) has these defects:

| File | Problems |
|---|---|
| `GrayLogTagger.cs` | Patterns are hardcoded (`log.,logger.,log?.,logger?.`); the whole incoming span is tagged, not the line; `TagsChanged` is never raised |
| `GrayLogClassifier.cs`, `GrayLogClassifierProvider.cs` | Unnecessary tagger + aggregator indirection; the classifier can match text directly |
| `GrayLogFormat.cs` | Color is read once from `DTE.Properties` at MEF composition time; changes require a restart |
| `OptionGrayLog.cs`, `GrayLogToolsOptionsPackage.cs` | Single color, no patterns, unused "Show Thumbs" option |
| `GrayLogGlyph*.cs/.xaml` | Glyph margin dot; dropped in v1 |
| `source.extension.vsixmanifest` | `InstallationTarget [16.0,17.0)`; does not install in VS 2022 or VS 2026 |

Decision: rewrite from scratch in a new SDK-style project. The old project is deleted from the working tree;
it remains available in git history.

## 3. Architecture

Classic in-process VSSDK editor extension (MEF). Editor classification is the supported, stable path for text coloring.

```
GrayLogs.slnx
├── src/GrayLog/                       VSIX project (SDK-style, net48 — required by in-process VS extensions)
│   ├── GrayLogPackage.cs              AsyncPackage: options page and toggle command registration
│   ├── GrayLogOptions.cs              Options page: rules text
│   ├── GrayLogSettings.cs             Settings store access, change notification
│   ├── GrayLogPackage.vsct            "Toggle GrayLog" command, menu placement, default key binding
│   ├── RuleParser.cs                  Rules text -> list of (Regex, style slot); reports invalid lines
│   ├── LineMatcher.cs                 Pure logic: which lines of a text are dimmed and with which slot (no VS types)
│   ├── GrayLogClassifier.cs           IClassifier + IClassifierProvider
│   ├── GrayLogFormats.cs              Three classification types and their default formats
│   └── source.extension.vsixmanifest
└── tests/GrayLog.Tests/               xUnit v3 + FluentAssertions 7.x (net10.0)
    ├── RuleParserTests.cs
    └── LineMatcherTests.cs
```

Dependencies: `Microsoft.VisualStudio.SDK` and `Microsoft.VSSDK.BuildTools` only. `Community.VisualStudio.Toolkit`
was considered and dropped: one options page and one command do not justify an extra dependency.

### 3.1 Rules format (Tools > Options > GrayLog > General)

Settings are global only and stored in the VS user settings store (collection `GrayLog`).
The options page has one property, `Rules` (multi-line string, edited with
`System.ComponentModel.Design.MultilineStringEditor`). The on/off state is changed only by the toggle command,
whose menu item shows a check mark while dimming is enabled.

One rule per line:

```
# Lines starting with '#' are comments; empty lines are ignored.
# Optional prefix "N: " selects style slot 1..3 (default 1). Use (?i) for case-insensitive matching.
(?i)\b_?log(ger)?\??\.
2: \bConsole\.Write(Line)?\(
2: \bDebug\.(Write|Assert)
```

- A multi-line text field was chosen over a table editor: it needs no custom UI, is easy to copy between machines,
  and supports comments.
- Invalid regex lines are reported when the options page is saved (message with line number) and skipped at runtime.
- Regexes are created once per settings change with `RegexOptions.CultureInvariant` and a 50 ms match timeout.

### 3.2 Appearance

Three classification types exported via MEF, visible in *Tools > Options > Environment > Fonts and Colors*:

| Slot | Name in Fonts and Colors | Default format |
|---|---|---|
| 1 | `GrayLog - Style 1` | Gray foreground `#808080` |
| 2 | `GrayLog - Style 2` | Same as slot 1, italic |
| 3 | `GrayLog - Style 3` | Same as slot 1 |

Format definitions use `Order(After = Priority.High)` so they override Roslyn syntax colors on the dimmed lines.
The spike also checks whether `ForegroundOpacity` alone (keeping syntax colors, only fading them) is merged by
VS 2026; if yes, slot 1 defaults to opacity 0.5 instead of a fixed color.

### 3.3 Matching and multi-line statements

`LineMatcher` works on plain strings so it is fully unit-testable.

1. A line is a **start line** when any rule matches it. The first matching rule defines the slot.
2. From the match position, the matcher scans forward for the first `(` and counts parentheses until the balance
   returns to zero. Parentheses inside `"..."`, `'...'`, `@"..."` and `$"..."` literals and after `//` are ignored.
   All lines up to the closing parenthesis are dimmed with the same slot.
3. The scan stops after 30 lines without balance, so an unbalanced line cannot dim the rest of the file.

Classifier integration:

- For each requested line, the classifier checks the line itself and scans back up to 30 lines for a start line
  whose statement reaches the requested line.
  (`ponytail:` backward scan per line; add a per-snapshot cache if profiling shows cost on large files.)
- On `ITextBuffer.Changed`, the classifier raises `ClassificationChanged` for the changed lines plus the following
  30 lines, because an edit can open or close a multi-line statement.
- On settings change or toggle, the classifier raises `ClassificationChanged` for the whole buffer. Open documents
  update without restart.

Parenthesis balancing is language-agnostic. It does not understand C# raw string literals (`"""..."""`),
`/* */` comments, or literal syntaxes of other languages; such cases only affect dimming.

### 3.4 Languages

`ContentType("code")`: all code editors. Matching is plain text, so no language-specific cost.
C# is the primary test target.

### 3.5 Toggle

- Command `Tools.ToggleGrayLog` in the *Tools* menu, labeled *Toggle GrayLog*.
- Default key binding proposed: `Ctrl+Alt+Shift+G` (checked for conflicts in the spike); users can rebind it in
  *Tools > Options > Environment > Keyboard*.
- The command flips the enabled state and saves it, so the state survives restarts.

### 3.6 Manifest

- Display name `GrayLog`, identity Id `GrayLogGlyphFactory` (kept from the old version), version `2.0.0` (Semantic Versioning; 1.x and 2019.x were the old extension).
- `InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[18.0,)"` with `ProductArchitecture` `amd64` and `arm64`.
- Prerequisite `Microsoft.VisualStudio.Component.CoreEditor` `[18.0,)`.

## 4. Implementation phases

0. **Cleanup.** Delete the old project files (sources, `.csproj`, `packages.config`, `app.config`, `Key.snk`,
   `xpush.bat`, glyph files). Keep `Example.GrayLogClassification.png` until a new screenshot exists.
1. **Spike (0.5 day).** New VSIX project for VS 2026; confirm install/debug in the Experimental Instance,
   `ForegroundOpacity` merging, key binding conflicts.
2. **Core (1 day).** `RuleParser` and `LineMatcher` with tests, including parenthesis balancing and literals.
3. **Editor integration (1 day).** Classifier, three format definitions, change notifications.
4. **Settings and toggle (1 day).** Options page with validation, toggle command, live update.
5. **Packaging (0.5 day).** Manifest, icon, README, new screenshot, local `.vsix` build.

Estimated total: 4 days of focused work.

## 5. Tests

- `RuleParserTests`: comments, empty lines, slot prefix, default slot, invalid regex reported with line number.
- `LineMatcherTests`: single-line match, `(?i)` flag, multi-line call dimmed to closing parenthesis,
  parentheses inside string literals and comments, 30-line limit on unbalanced statements, first rule wins.
- All tests carry `[Trait("Category", ...)]`.
- The classifier and options page are checked manually in the Experimental Instance.

## 6. Risks

| Risk | Mitigation |
|---|---|
| Regex on every visible line slows the editor on large files | Only requested spans are classified; 50 ms regex timeout; add caching if profiling requires it |
| Parenthesis balancing fails on unusual syntax | 30-line limit; affects only dimming |
| Roslyn or other extensions override the style | `Order(After = Priority.High)`; verified in the spike |
| Template or SDK differences in VS 2026 | Spike in phase 1 |

## 7. Decisions

| # | Question | Answer |
|---|---|---|
| Q1 | Supported VS versions | VS 2026 and later |
| Q2 | Appearance model | A — fixed style slots in *Fonts and Colors* |
| Q3 | What is dimmed | The whole line |
| Q4 | Settings location | Global Tools > Options only; format chosen by the implementer (multi-line text, section 3.1) |
| Q5 | Multi-line statements | Yes, via parenthesis balancing |
| Q6 | Languages | C# primary; other languages included, because generic matching costs nothing extra |
| Q7 | Glyph margin marker | Dropped |
| Q8 | On/off toggle | Yes: menu command and keyboard shortcut |
| Q9 | Old code | Delete from the working tree |
| Q10 | Distribution and name | Local `.vsix` only; keep the name GrayLog |

## 8. Implementation notes

- Solution: `GrayLogs.slnx`. The VSIX builds with Visual Studio 2026 MSBuild:
  `MSBuild.exe GrayLogs.slnx -restore -p:Configuration=Release`; output `src/GrayLog/bin/Release/net48/GrayLog.vsix`.
- The VSIX project sets `VSSDKBuildToolsAutoSetup=true`, which imports the VSSDK targets into the SDK-style project.
- Tests: `dotnet test --project tests/GrayLog.Tests`. The test project targets `net10.0` and compiles `RuleParser.cs`
  and `LineMatcher.cs` directly, because they have no VS dependencies. `global.json` opts into
  Microsoft.Testing.Platform, required by `dotnet test` with xunit.v3 on the .NET 10 SDK.
  AutoFixture and NSubstitute are not referenced: the tested code has no dependencies to substitute.
- The preview image `Example.GrayLogClassification.png` is removed; a new screenshot is still to be added.
- Not verified yet (requires running Visual Studio): classification colors over Roslyn highlighting,
  `ForegroundOpacity` merging, the `Ctrl+Alt+Shift+G` binding, and the options page in the VS 2026 options UI.
- `.github/workflows/codeql.yml` uses autobuild on `ubuntu-latest`, which cannot build a .NET Framework VSIX.
