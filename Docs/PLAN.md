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
- Each pattern uses one of three style slots; colors are edited in the GrayLog options page.
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
│   ├── GrayLogOptions.cs              Options page (UIElementDialogPage)
│   ├── GrayLogOptionsControl.xaml(.cs) Options UI: rules table, test field
│   ├── GrayLogSettings.cs             Settings store access, change notification
│   ├── GrayLogPackage.vsct            "Toggle GrayLog" command, menu placement, default key binding
│   ├── RuleParser.cs                  Rule definitions: defaults, storage format, compilation
│   ├── LineMatcher.cs                 Pure logic: which lines of a text are dimmed and with which slot (no VS types)
│   ├── GrayLogClassifier.cs           IClassifier + IClassifierProvider
│   ├── GrayLogFormats.cs              Style slots: classification types, style settings, format map update
│   └── source.extension.vsixmanifest
└── tests/GrayLog.Tests/               xUnit v3 + FluentAssertions 7.x (net10.0)
    ├── RuleParserTests.cs
    └── LineMatcherTests.cs
```

Dependencies: `Microsoft.VisualStudio.SDK` and `Microsoft.VSSDK.BuildTools` only. `Community.VisualStudio.Toolkit`
was considered and dropped: one options page and one command do not justify an extra dependency.

### 3.1 Rules and options page (Tools > Options > GrayLog > General)

Settings are global only and stored in the VS user settings store (collection `GrayLog`, keys `Enabled` and
`RuleDefinitions`). The options page (`UIElementDialogPage` with a WPF control, VS-themed dialog styles) contains:

- a checkbox that enables or disables dimming (the same state as the toggle command);
- a table of rules with the columns *On*, *Name*, *Regular expression*, *Ignore case*, *Style* (1..3);
- buttons *Add rule*, *Remove*, *Move up*, *Move down*, *Reset to defaults*;
- a *Try a line of code* field that shows which rule dims the entered line;
- a note where the colors are edited.

Predefined rules:

| Name | Pattern | Ignore case | Style | On |
|---|---|---|---|---|
| Logger calls | `\b_?log(ger)?\??\.` | yes | 1 | yes |
| Console output | `\bConsole\.Write(Line)?\(` | no | 2 | no |
| Debug / Trace output | `\b(Debug\|Trace)\.(Write\|WriteLine\|Print\|Assert\|Fail)\w*\(` | no | 2 | no |

- Version 2.0.0 used a single multi-line text field; it was replaced in 2.1.0 because it was not intuitive.
- Invalid patterns are shown under the table while editing and block *OK* with a message; at runtime they are skipped.
- Regexes are created once per settings change with `RegexOptions.CultureInvariant` and a 50 ms match timeout.
- Storage format: one rule per line, tab-separated `enabled, style, ignoreCase, name, pattern`.

### 3.2 Appearance

Three classification types exported via MEF (`GrayLog - Style 1..3`), hidden from *Fonts and Colors*
(`UserVisible(false)`). Their color and italic flag are edited in the *Styles* section of the GrayLog options page
(color name or `#RRGGBB`, a *Choose...* button with the Windows color dialog, a live preview) and stored in the
GrayLog settings collection (`Style{N}Color`, `Style{N}Italic`).

The package applies them to the editor's `text` classification format map (`IClassificationFormatMapService`)
at startup and after every settings change, so all settings are in one place.

| Slot | Default |
|---|---|
| 1 | Olive |
| 2 | Olive, italic |
| 3 | Gray |

Format definitions use `Order(After = Priority.High)` so they override Roslyn syntax colors on the dimmed lines.

### 3.3 Matching and multi-line statements

`LineMatcher` works on plain strings so it is fully unit-testable.

1. A line is a **start line** when any rule matches it. The first matching rule defines the slot.
2. From the match position, the matcher scans forward for the first `(` and counts parentheses until the balance
   returns to zero. Parentheses inside `"..."`, `'...'`, `@"..."` and `$"..."` literals and after `//` are ignored.
   All lines up to the closing parenthesis are dimmed with the same slot.
3. The scan stops after 30 lines without balance, so an unbalanced line cannot dim the rest of the file.

Classifier integration:

- For a requested span, `LineMatcher.GetSlots` makes one pass from 30 lines above the span to its end. Each line
  text is read and matched at most once per call (a test checks this).
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

- `RuleParserTests`: defaults, disabled/empty rules, style clamping, ignore case, invalid pattern reported with rule number, storage round trip.
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
| Q2 | Appearance model | A — fixed style slots (edited in *Fonts and Colors* in 2.0.0, in the GrayLog options page since 2.1.0) |
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

### Resource review (2.1.0)

Static review of object lifetimes; no runtime profiling in Visual Studio yet.

- Classifier: one per text buffer, stored in `buffer.Properties`; it subscribes only to its own buffer's
  `Changed` event, so it is collected together with the buffer.
- Settings change notification: a static list of `WeakReference<GrayLogClassifier>`, pruned when a classifier
  is created; the static `GrayLogSettings.Changed` handler captures no instance.
- Options page: one WPF control per package lifetime; it edits clones of the rules.
- Regexes are not `RegexOptions.Compiled` and not stored in the static regex cache; replaced instances are collected.
- Fixed in 2.1.0: classification read each line text up to 31 times per request (backward scan per line);
  now each line is read and matched once per request.
- A pattern that exceeds the 50 ms match timeout is disabled until settings change (`Rule.MarkTimedOut`, thread-safe),
  and the package shows a one-time warning, so a pathological pattern costs at most one timeout.
- Runtime check: open and close many documents in the Experimental Instance, take a heap snapshot
  (Visual Studio Diagnostic Tools or PerfView), and verify that the number of `GrayLogClassifier` instances
  matches the number of open code buffers.
