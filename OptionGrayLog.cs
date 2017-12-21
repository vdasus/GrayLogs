using System.ComponentModel;
using System.Drawing;
using Microsoft.VisualStudio.Shell;

namespace GrayLog
{
    public class OptionGrayLog : DialogPage
    {
        [Category("GrayLog")]
        [DisplayName("Show thumbs")]
        [Description("Show thumbs")]
        public bool OptionBool { get; set; } = true;

        [Category("GrayLog")]
        [DisplayName("My Color Option")]
        [Description("My Color option")]
        public Color OptionColor { get; set; } = Color.DarkOliveGreen;
    }
}