using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.Drawing;

namespace GrayLog
{
    /*[Guid(GuidList.guidMyToolsOptionsPkgString)]*/
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    /*[ProvideProfile(typeof(OptionGrayLog),
        "GrayLog", "General", 106, 107, isToolsOptionPage: true, DescriptionResourceID = 108)]*/
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionGrayLog), "GrayLog", "General", 0, 0, true)]
    [Guid(PACKAGE_GUID_STRING)]
    public sealed class GrayLogToolsOptionsPackage : Package
    {
        /*public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165386B64398";*/
        public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165327B64398";
    
        public bool OptionBool
        {
            get
            {
                OptionGrayLog page = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return page.OptionBool;
            }
        }

        public Color OptionColor
        {
            get
            {
                OptionGrayLog page = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return page.OptionColor;
            }
        }
    }
}
