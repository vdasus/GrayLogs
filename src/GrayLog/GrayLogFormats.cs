using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace GrayLog
{
    /// <summary>Color and font style of one style slot, as edited in the GrayLog options page.</summary>
    public sealed class StyleSettings : INotifyPropertyChanged
    {
        private string _color;
        private bool _italic;

        public StyleSettings(int number, string color, bool italic)
        {
            Number = number;
            _color = color;
            _italic = italic;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Number { get; }

        public string Title => $"Style {Number}";

        /// <summary>Color name (for example "Olive") or "#RRGGBB".</summary>
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Brush));
            }
        }

        public bool Italic
        {
            get => _italic;
            set
            {
                _italic = value;
                OnPropertyChanged(nameof(Italic));
                OnPropertyChanged(nameof(FontStyle));
            }
        }

        public Brush Brush => TryParseColor(Color, out var color) ? new SolidColorBrush(color) : Brushes.Transparent;

        public FontStyle FontStyle => Italic ? FontStyles.Italic : FontStyles.Normal;

        public StyleSettings Clone() => new StyleSettings(Number, Color, Italic);

        public static bool TryParseColor(string text, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            try
            {
                color = (Color)ColorConverter.ConvertFromString(text.Trim());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Three style slots. Their colors are edited in the GrayLog options page and applied to the editor's
    /// classification format map, so they are not listed in Fonts and Colors.
    /// </summary>
    internal sealed class GrayLogFormats
    {
        public const string Style1 = "GrayLog - Style 1";
        public const string Style2 = "GrayLog - Style 2";
        public const string Style3 = "GrayLog - Style 3";

        public static readonly string[] ClassificationNames = { Style1, Style2, Style3 };

        private static IClassificationFormatMapService _formatMapService;
        private static IClassificationTypeRegistryService _registry;

#pragma warning disable 0649 // Never assigned: the fields only carry MEF export metadata.
        [Export, Name(Style1)] internal ClassificationTypeDefinition Style1Type;
        [Export, Name(Style2)] internal ClassificationTypeDefinition Style2Type;
        [Export, Name(Style3)] internal ClassificationTypeDefinition Style3Type;
#pragma warning restore 0649

        public static List<StyleSettings> CreateDefaultStyles() => new List<StyleSettings>
        {
            new StyleSettings(1, "Olive", false),
            new StyleSettings(2, "Olive", true),
            new StyleSettings(3, "Gray", false),
        };

        /// <summary>Applies the configured styles now and after every settings change. Only the first call has an effect.</summary>
        public static void Initialize(IClassificationFormatMapService formatMapService, IClassificationTypeRegistryService registry)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_formatMapService != null) return;

            _formatMapService = formatMapService;
            _registry = registry;
            GrayLogSettings.Changed += (sender, args) => Apply();
            Apply();
        }

        private static void Apply()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_formatMapService == null) return;

            var map = _formatMapService.GetClassificationFormatMap("text");
            map.BeginBatchUpdate();
            try
            {
                foreach (var style in GrayLogSettings.Styles)
                {
                    if (!StyleSettings.TryParseColor(style.Color, out var color)) continue;
                    var type = _registry.GetClassificationType(ClassificationNames[style.Number - 1]);
                    var properties = map.GetTextProperties(type).SetForeground(color).SetItalic(style.Italic);
                    map.SetTextProperties(type, properties);
                }
            }
            finally
            {
                map.EndBatchUpdate();
            }
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style1)]
    [Name(GrayLogFormats.Style1)]
    [UserVisible(false)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle1Format : ClassificationFormatDefinition
    {
        public GrayLogStyle1Format()
        {
            DisplayName = GrayLogFormats.Style1;
            ForegroundColor = Colors.Olive;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style2)]
    [Name(GrayLogFormats.Style2)]
    [UserVisible(false)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle2Format : ClassificationFormatDefinition
    {
        public GrayLogStyle2Format()
        {
            DisplayName = GrayLogFormats.Style2;
            ForegroundColor = Colors.Olive;
            IsItalic = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = GrayLogFormats.Style3)]
    [Name(GrayLogFormats.Style3)]
    [UserVisible(false)]
    [Order(After = Priority.High)]
    internal sealed class GrayLogStyle3Format : ClassificationFormatDefinition
    {
        public GrayLogStyle3Format()
        {
            DisplayName = GrayLogFormats.Style3;
            ForegroundColor = Colors.Gray;
        }
    }
}
