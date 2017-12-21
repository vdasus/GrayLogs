using System.ComponentModel.Composition;
using System.Windows.Media;
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
            DisplayName = "GrayLog Text"; //human readable version of the name
            BackgroundOpacity = 0;

            ForegroundColor = Color.FromRgb(84, 112, 109);
            //ForegroundColor = Color.FromRgb(115, 132, 84);
            //BackgroundColor = Colors.Orange;
        }
    }
}
