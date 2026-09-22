using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GrayLog
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(GrayLogOptions), "GrayLog", "General", 0, 0, false)]
    // Load at startup so saved settings reach the classifier before the first document opens.
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class GrayLogPackage : AsyncPackage
    {
        public const string PackageGuidString = "876d4e51-d169-4754-a8d3-8451e4be2456";

        private static readonly Guid CommandSet = new Guid("5b0f3c2e-8a4d-4f7e-9c61-2d7e3a9b1f40");
        private const int ToggleCommandId = 0x0100;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            GrayLogSettings.Load(this);

            if (!(await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService)) return;
            var toggle = new OleMenuCommand(OnToggle, new CommandID(CommandSet, ToggleCommandId));
            toggle.BeforeQueryStatus += (sender, args) => toggle.Checked = GrayLogSettings.Enabled;
            commandService.AddCommand(toggle);
        }

        private static void OnToggle(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GrayLogSettings.Save(!GrayLogSettings.Enabled, GrayLogSettings.RulesText);
        }
    }
}
