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
            Color color = Color.FromRgb(84, 112, 109);

            try
            {
                DTE dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE;
                Properties props = dte?.Properties["GrayLog", "General"];
                
                Property pathProperty = props?.Item("OptionColor");
                if (pathProperty?.Value != null)
                {
                    color = UIntToColor((uint)pathProperty.Value);
                }
            }
            catch (Exception)
            {
                //
            }
            
            DisplayName = "GrayLog Text"; //human readable version of the name
            BackgroundOpacity = 0;
            ForegroundColor = color;
        }

        private static Color UIntToColor(uint color)
        {
            return Color.FromArgb(
                (byte) (color >> 24), 
                (byte) (color >> 0), 
                (byte) (color >> 8), 
                (byte) (color >> 16));
        }
    }
}
