using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GrayLog
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    /*[Guid(GuidList.guidMyToolsOptionsPkgString)]*/
    [ProvideOptionPage(typeof(OptionGrayLog),
        "GrayLog", "General", 0, 0, true)]
    [Guid(PACKAGE_GUID_STRING)]
    public sealed class GrayLogToolsOptionsPackage : Package
    {
        public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165327B64398";
    }
}
