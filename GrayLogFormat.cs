using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace GrayLog
{
    /// <summary>
    /// Set the display values for the classification
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "GrayLog")]
    [Name("GrayLog")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogFormat : ClassificationFormatDefinition
    {
        public GrayLogFormat()
        {
            /*Color clr = Color.FromRgb(84, 112, 109);*/
            Color clr = Color.FromRgb(84, 0, 0);

            try
            {
                //https://stackoverflow.com/questions/6641899/how-to-encapsulate-user-setting-options-page-in-visual-studio-2010-addin
                //https://msdn.microsoft.com/en-us/library/bb166195.aspx
                DTE dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE;
                // Access options page
                Properties props = dte?.Properties["GrayLog", "General"];
                
                Property pathProperty = props?.Item("OptionColor");
                if (pathProperty?.Value != null)
                {
                    clr = UIntToColor((uint)pathProperty?.Value);
                }

                /*GrayLogToolsOptionsPackage myToolsOptionsPackage = this.package as GrayLogToolsOptionsPackage;*/
            }
            catch (Exception ex)
            {
                var tmp = ex.Message;
            }
            
            DisplayName = "GrayLog Text"; //human readable version of the name
            BackgroundOpacity = 0;
            ForegroundColor = clr;
            //BackgroundColor = Colors.Orange;
        }

        private static Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var b = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var r = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
