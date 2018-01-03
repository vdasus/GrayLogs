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
    [Name("GrayLogText")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogFormat : ClassificationFormatDefinition
    {
        public GrayLogFormat()
        {
            try
            {
                //https://stackoverflow.com/questions/6641899/how-to-encapsulate-user-setting-options-page-in-visual-studio-2010-addin
                //https://msdn.microsoft.com/en-us/library/bb166195.aspx
                DTE dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE;
                // Access options page
                Properties props = dte?.get_Properties("GrayLog", "General");
                
                var pathProperty = props?.Item("OptionColor");
                Color path = (Color) pathProperty?.Value;
            }
            catch (Exception ex)
            {
                var tmp = ex.Message;
            }
            
            DisplayName = "GrayLog Text"; //human readable version of the name
            BackgroundOpacity = 0;
            
            ForegroundColor = Color.FromRgb(84, 112, 109);
            //ForegroundColor = Color.FromRgb(115, 132, 84);
            //BackgroundColor = Colors.Orange;
        }
    }
}
