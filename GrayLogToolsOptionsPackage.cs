using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace GrayLog
{
    /*[Guid(GuidList.guidMyToolsOptionsPkgString)]*/
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionGrayLog), "GrayLog", "General", 0, 0, true)]
    [Guid(PACKAGE_GUID_STRING)]
    public sealed class GrayLogToolsOptionsPackage : Package
    {
        public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165327B64398";

        public Color OptionColor
        {
            get
            {
                OptionGrayLog page = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return Color.FromArgb(page.OptionColor.A, page.OptionColor.R, page.OptionColor.G, page.OptionColor.B);
            }
        }
    }
}
