﻿using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GrayLog
{
    /*[Guid(GuidList.guidMyToolsOptionsPkgString)]*/
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "2019.3", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionGrayLog), "GrayLog", "General", 0, 0, true)]
    [Guid(PACKAGE_GUID_STRING)]
    public sealed class GrayLogToolsOptionsPackage : AsyncPackage
    {
        /*public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165386B64398";*/
        /*public const string PACKAGE_GUID_STRING = "2511FF91-C7ED-4406-A28C-165327B64398";*/
        public const string PACKAGE_GUID_STRING = "876D4E51-D169-4754-A8D3-8451E4BE2456";

        public bool OptionIsSdhowThumbs
        {
            get
            {
                OptionGrayLog thumb = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return thumb.OptionBool;
            }
        }

        public Color OptionColor
        {
            get
            {
                OptionGrayLog color = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return color.OptionColor;
            }
        }
    }
}
