using System;
using System.Collections.Generic;
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

        private static WritableSettingsStore _store;

        public static bool Enabled { get; private set; } = true;

        public static string RulesText { get; private set; } = RuleParser.DefaultRules;

        public static IReadOnlyList<Rule> Rules { get; private set; } = RuleParser.Parse(RuleParser.DefaultRules);

        /// <summary>Raised on the UI thread after settings are loaded or changed.</summary>
        public static event EventHandler Changed;

        public static void Load(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _store = new ShellSettingsManager(serviceProvider).GetWritableSettingsStore(SettingsScope.UserSettings);
            Apply(
                _store.GetBoolean(Collection, nameof(Enabled), true),
                _store.GetString(Collection, nameof(RulesText), RuleParser.DefaultRules));
        }

        public static void Save(bool enabled, string rulesText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_store != null)
            {
                if (!_store.CollectionExists(Collection)) _store.CreateCollection(Collection);
                _store.SetBoolean(Collection, nameof(Enabled), enabled);
                _store.SetString(Collection, nameof(RulesText), rulesText ?? string.Empty);
            }

            Apply(enabled, rulesText);
        }

        private static void Apply(bool enabled, string rulesText)
        {
            Enabled = enabled;
            RulesText = rulesText ?? string.Empty;
            Rules = RuleParser.Parse(RulesText);
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
}
