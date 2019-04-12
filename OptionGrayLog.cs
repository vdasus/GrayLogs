using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GrayLog
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class OptionGrayLog : DialogPage
    {
        [Category("GrayLog")]
        [DisplayName("Show Thumbs")]
        [Description("Show thumbs on lines with log (not implemented yet)")]
        public bool OptionBool { get; set; } = true;

        [Category("GrayLog")]
        [DisplayName("Foreground Color")]
        [Description("Foreground Color of log lines")]
        public Color OptionColor { get; set; } = Color.DarkOliveGreen;
    }
}