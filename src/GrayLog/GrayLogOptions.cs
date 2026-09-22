using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GrayLog
{
    /// <summary>
    /// Tools > Options > GrayLog > General. Values live in <see cref="GrayLogSettings"/>, not in the page.
    /// </summary>
    internal sealed class GrayLogOptions : UIElementDialogPage
    {
        private GrayLogOptionsControl _control;

        protected override UIElement Child => _control ?? (_control = CreateControl());

        private static GrayLogOptionsControl CreateControl()
        {
            return new GrayLogOptionsControl { DimmingEnabled = GrayLogSettings.Enabled, Rules = GrayLogSettings.Definitions };
        }

        public override void LoadSettingsFromStorage()
        {
            if (_control == null) return;
            _control.DimmingEnabled = GrayLogSettings.Enabled;
            _control.Rules = GrayLogSettings.Definitions;
        }

        public override void SaveSettingsToStorage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_control != null) GrayLogSettings.Save(_control.DimmingEnabled, _control.Rules);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_control != null)
            {
                var errors = new List<string>();
                RuleParser.Compile(_control.Rules, errors);
                if (errors.Count > 0)
                {
                    VsShellUtilities.ShowMessageBox(Site, string.Join("\n", errors), "GrayLog: invalid rules",
                        OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    e.ApplyBehavior = ApplyKind.CancelNoNavigate;
                    return;
                }
            }

            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Discard unsaved edits (Cancel) and pick up changes made by the toggle command before the next opening.
            LoadSettingsFromStorage();
            base.OnClosed(e);
        }
    }
}
