using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace GrayLog
{
    /// <summary>
    /// Three style slots. Users edit them in Tools > Options > Environment > Fonts and Colors.
    /// </summary>
    internal sealed class GrayLogFormats
    {
        public const string Style1 = "GrayLog - Style 1";
        public const string Style2 = "GrayLog - Style 2";
        public const string Style3 = "GrayLog - Style 3";

        public static readonly string[] ClassificationNames = { Style1, Style2, Style3 };

        internal static readonly Color DefaultColor = Color.FromRgb(128, 128, 128);

#pragma warning disable 0649 // Never assigned: the fields only carry MEF export metadata.
        [Export, Name(Style1)] internal ClassificationTypeDefinition Style1Type;
        [Export, Name(Style2)] internal ClassificationTypeDefinition Style2Type;
        [Export, Name(Style3)] internal ClassificationTypeDefinition Style3Type;
#pragma warning restore 0649
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style1)]
    [Name(GrayLogFormats.Style1)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle1Format : ClassificationFormatDefinition
    {
        public GrayLogStyle1Format()
        {
            DisplayName = GrayLogFormats.Style1;
            ForegroundColor = GrayLogFormats.DefaultColor;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style2)]
    [Name(GrayLogFormats.Style2)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle2Format : ClassificationFormatDefinition
    {
        public GrayLogStyle2Format()
        {
            DisplayName = GrayLogFormats.Style2;
            ForegroundColor = GrayLogFormats.DefaultColor;
            IsItalic = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style3)]
    [Name(GrayLogFormats.Style3)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle3Format : ClassificationFormatDefinition
    {
        public GrayLogStyle3Format()
        {
            DisplayName = GrayLogFormats.Style3;
            ForegroundColor = GrayLogFormats.DefaultColor;
        }
    }
}
