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
        public Color OptionColor
        {
            get
            {
                OptionGrayLog page = (OptionGrayLog)GetDialogPage(typeof(OptionGrayLog));
                return Color.FromArgb(page.OptionColor.A, page.OptionColor.R, page.OptionColor.G, page.OptionColor.B);
            }
        }

        public GrayLogFormat()
        {
            DisplayName = "GrayLog Text"; //human readable version of the name
            BackgroundOpacity = 0;

            ForegroundColor = Color.FromRgb(115, 132, 84);
            //ForegroundColor = Color.FromRgb(115, 132, 84);
            //BackgroundColor = Colors.Orange;
        }
    }
}
