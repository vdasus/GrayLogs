using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace GrayLog
{
    /// <summary>
    /// Global settings, persisted in the VS user settings store. Defaults apply until the package loads them.
    /// </summary>
    internal static class GrayLogSettings
    {
        private const string Collection = "GrayLog";
        private const string EnabledKey = "Enabled";
        private const string RulesKey = "RuleDefinitions";

        private static WritableSettingsStore _store;

        public static bool Enabled { get; private set; } = true;

        /// <summary>Rules as edited by the user. Treat as read-only; the options page edits clones.</summary>
        public static IReadOnlyList<RuleDefinition> Definitions { get; private set; } = RuleParser.CreateDefaults();

        public static IReadOnlyList<Rule> Rules { get; private set; } = RuleParser.Compile(RuleParser.CreateDefaults());

        /// <summary>One entry per style slot. Treat as read-only; the options page edits clones.</summary>
        public static IReadOnlyList<StyleSettings> Styles { get; private set; } = GrayLogFormats.CreateDefaultStyles();

        /// <summary>Raised on the UI thread after settings are loaded or changed.</summary>
        public static event EventHandler Changed;

        public static void Load(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _store = new ShellSettingsManager(serviceProvider).GetWritableSettingsStore(SettingsScope.UserSettings);
            var definitions = _store.PropertyExists(Collection, RulesKey)
                ? RuleParser.Deserialize(_store.GetString(Collection, RulesKey))
                : RuleParser.CreateDefaults();
            var styles = GrayLogFormats.CreateDefaultStyles()
                .Select(d => new StyleSettings(d.Number,
                    _store.GetString(Collection, ColorKey(d.Number), d.Color),
                    _store.GetBoolean(Collection, ItalicKey(d.Number), d.Italic)))
                .ToList();
            Apply(_store.GetBoolean(Collection, EnabledKey, true), definitions, styles);
        }

        public static void Save(bool enabled, IEnumerable<RuleDefinition> definitions, IEnumerable<StyleSettings> styles)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var definitionsCopy = definitions.Select(d => d.Clone()).ToList();
            var stylesCopy = styles.Select(s => s.Clone()).ToList();
            if (_store != null)
            {
                if (!_store.CollectionExists(Collection)) _store.CreateCollection(Collection);
                _store.SetBoolean(Collection, EnabledKey, enabled);
                _store.SetString(Collection, RulesKey, RuleParser.Serialize(definitionsCopy));
                foreach (var style in stylesCopy)
                {
                    _store.SetString(Collection, ColorKey(style.Number), style.Color ?? "");
                    _store.SetBoolean(Collection, ItalicKey(style.Number), style.Italic);
                }
            }

            Apply(enabled, definitionsCopy, stylesCopy);
        }

        private static string ColorKey(int number) => $"Style{number}Color";

        private static string ItalicKey(int number) => $"Style{number}Italic";

        private static void Apply(bool enabled, List<RuleDefinition> definitions, List<StyleSettings> styles)
        {
            Enabled = enabled;
            Definitions = definitions;
            Rules = RuleParser.Compile(definitions);
            Styles = styles;
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
}
