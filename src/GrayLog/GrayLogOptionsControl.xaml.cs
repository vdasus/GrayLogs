using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GrayLog
{
    /// <summary>Editor for the rules; works on clones, so nothing changes until the page is applied.</summary>
    public partial class GrayLogOptionsControl : UserControl
    {
        private readonly ObservableCollection<RuleDefinition> _rules = new ObservableCollection<RuleDefinition>();

        public GrayLogOptionsControl()
        {
            InitializeComponent();
            RulesGrid.ItemsSource = _rules;
        }

        public bool DimmingEnabled
        {
            get => EnabledBox.IsChecked == true;
            set => EnabledBox.IsChecked = value;
        }

        public IReadOnlyList<RuleDefinition> Rules
        {
            get
            {
                RulesGrid.CommitEdit(DataGridEditingUnit.Row, true);
                return _rules.ToList();
            }
            set
            {
                _rules.Clear();
                foreach (var rule in value) _rules.Add(rule.Clone());
                Refresh();
            }
        }

        public IReadOnlyList<StyleSettings> Styles
        {
            get => (IReadOnlyList<StyleSettings>)StylesList.ItemsSource;
            set => StylesList.ItemsSource = value.Select(style => style.Clone()).ToList();
        }

        private void OnChooseColor(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is StyleSettings style)) return;

            using (var dialog = new System.Windows.Forms.ColorDialog { FullOpen = true })
            {
                if (StyleSettings.TryParseColor(style.Color, out var current))
                {
                    dialog.Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B);
                }

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    style.Color = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                }
            }
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            var rule = new RuleDefinition { Name = "New rule", Pattern = @"\bMyLogger\." };
            _rules.Add(rule);
            RulesGrid.SelectedItem = rule;
            RulesGrid.ScrollIntoView(rule);
            Refresh();
        }

        private void OnRemove(object sender, RoutedEventArgs e)
        {
            if (RulesGrid.SelectedItem is RuleDefinition rule) _rules.Remove(rule);
            Refresh();
        }

        private void OnMoveUp(object sender, RoutedEventArgs e) => Move(-1);

        private void OnMoveDown(object sender, RoutedEventArgs e) => Move(1);

        private void Move(int offset)
        {
            if (!(RulesGrid.SelectedItem is RuleDefinition rule)) return;
            RulesGrid.CommitEdit(DataGridEditingUnit.Row, true);
            var index = _rules.IndexOf(rule);
            var target = index + offset;
            if (target < 0 || target >= _rules.Count) return;
            _rules.Move(index, target);
            RulesGrid.SelectedItem = rule;
            Refresh();
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            Rules = RuleParser.CreateDefaults();
            Styles = GrayLogFormats.CreateDefaultStyles();
        }

        private void OnInputChanged(object sender, TextChangedEventArgs e) => Refresh();

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Bindings update after this event, so re-evaluate once they are done.
#pragma warning disable VSTHRD001 // Already on the UI thread; this only defers the call until the edit is committed.
            _ = Dispatcher.BeginInvoke(new Action(Refresh), DispatcherPriority.Background);
#pragma warning restore VSTHRD001
        }

        private void Refresh()
        {
            if (SampleResult == null) return;

            var errors = new List<string>();
            var rules = RuleParser.Compile(_rules, errors);
            ErrorsText.Text = string.Join("\n", errors);
            ErrorsText.Visibility = errors.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            var sample = SampleBox.Text ?? "";
            var rule = rules.FirstOrDefault(r => IsMatch(r, sample));
            SampleResult.Text = rule == null
                ? "Not dimmed."
                : $"Dimmed by rule \"{rule.Name}\" with GrayLog - Style {rule.Slot + 1}.";
        }

        private static bool IsMatch(Rule rule, string text)
        {
            try
            {
                return rule.Pattern.IsMatch(text);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
