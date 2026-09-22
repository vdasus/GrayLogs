using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GrayLog
{
    /// <summary>
    /// Tools > Options > GrayLog > General. Values live in <see cref="GrayLogSettings"/>, not in the page.
    /// </summary>
    internal sealed class GrayLogOptions : DialogPage
    {
        [Category("GrayLog")]
        [DisplayName("Rules")]
        [Description("One regex per line. Optional prefix \"N: \" selects style slot 1..3 (default 1); " +
                     "colors are in Environment > Fonts and Colors as \"GrayLog - Style N\". " +
                     "Use (?i) for case-insensitive matching. Lines starting with '#' are comments.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Rules { get; set; }

        public override void LoadSettingsFromStorage()
        {
            Rules = GrayLogSettings.RulesText;
        }

        public override void SaveSettingsToStorage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GrayLogSettings.Save(GrayLogSettings.Enabled, Rules);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var errors = new List<string>();
            RuleParser.Parse(Rules, errors);
            if (errors.Count > 0)
            {
                VsShellUtilities.ShowMessageBox(Site, string.Join("\n", errors), "GrayLog: invalid rules",
                    OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                e.ApplyBehavior = ApplyKind.CancelNoNavigate;
                return;
            }

            base.OnApply(e);
        }
    }
}
