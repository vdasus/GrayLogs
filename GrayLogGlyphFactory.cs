using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace GrayLog
{
    /// <summary>
    /// This class implements IGlyphFactory, which provides the visual
    /// element that will appear in the glyph margin.
    /// </summary>
    internal class GrayLogGlyphFactory : IGlyphFactory
    {

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            return new GrayLogGlyph();
        }
    }
}
