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

        /// <summary>Raised on the UI thread after settings are loaded or changed.</summary>
        public static event EventHandler Changed;

        public static void Load(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _store = new ShellSettingsManager(serviceProvider).GetWritableSettingsStore(SettingsScope.UserSettings);
            var definitions = _store.PropertyExists(Collection, RulesKey)
                ? RuleParser.Deserialize(_store.GetString(Collection, RulesKey))
                : RuleParser.CreateDefaults();
            Apply(_store.GetBoolean(Collection, EnabledKey, true), definitions);
        }

        public static void Save(bool enabled, IEnumerable<RuleDefinition> definitions)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var copy = definitions.Select(d => d.Clone()).ToList();
            if (_store != null)
            {
                if (!_store.CollectionExists(Collection)) _store.CreateCollection(Collection);
                _store.SetBoolean(Collection, EnabledKey, enabled);
                _store.SetString(Collection, RulesKey, RuleParser.Serialize(copy));
            }

            Apply(enabled, copy);
        }

        private static void Apply(bool enabled, List<RuleDefinition> definitions)
        {
            Enabled = enabled;
            Definitions = definitions;
            Rules = RuleParser.Compile(definitions);
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
}
