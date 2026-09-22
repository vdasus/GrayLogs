using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace GrayLog
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    internal sealed class GrayLogClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new GrayLogClassifier(buffer, ClassificationRegistry));
        }
    }

    /// <summary>
    /// Classifies whole lines selected by <see cref="LineMatcher"/> with the GrayLog style of the matching rule.
    /// </summary>
    internal sealed class GrayLogClassifier : IClassifier
    {
        // Weak references: the static settings event must not keep closed buffers alive.
        private static readonly List<WeakReference<GrayLogClassifier>> Instances = new List<WeakReference<GrayLogClassifier>>();

        private readonly ITextBuffer _buffer;
        private readonly IClassificationType[] _types;

        static GrayLogClassifier()
        {
            GrayLogSettings.Changed += (sender, args) => RefreshAll();
        }

        internal GrayLogClassifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            _buffer = buffer;
            _types = GrayLogFormats.ClassificationNames.Select(registry.GetClassificationType).ToArray();
            _buffer.Changed += OnBufferChanged;

            lock (Instances)
            {
                Instances.RemoveAll(reference => !reference.TryGetTarget(out _));
                Instances.Add(new WeakReference<GrayLogClassifier>(this));
            }
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var result = new List<ClassificationSpan>();
            var rules = GrayLogSettings.Rules;
            if (!GrayLogSettings.Enabled || rules.Count == 0) return result;

            var snapshot = span.Snapshot;
            string GetLine(int number) => snapshot.GetLineFromLineNumber(number).GetText();

            var first = snapshot.GetLineNumberFromPosition(span.Start);
            var last = snapshot.GetLineNumberFromPosition(span.End);
            for (var number = first; number <= last; number++)
            {
                var slot = LineMatcher.GetSlot(rules, GetLine, snapshot.LineCount, number);
                if (slot < 0) continue;

                result.Add(new ClassificationSpan(snapshot.GetLineFromLineNumber(number).Extent, _types[slot]));
            }

            return result;
        }

        private static void RefreshAll()
        {
            GrayLogClassifier[] alive;
            lock (Instances)
            {
                alive = Instances.Select(reference => reference.TryGetTarget(out var target) ? target : null)
                    .Where(target => target != null)
                    .ToArray();
            }

            foreach (var classifier in alive)
            {
                var snapshot = classifier._buffer.CurrentSnapshot;
                classifier.ClassificationChanged?.Invoke(classifier, new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
            }
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // An edit can open or close a multi-line statement, so the following lines may change too.
            var snapshot = e.After;
            foreach (var change in e.Changes)
            {
                var startLine = snapshot.GetLineFromPosition(change.NewPosition);
                var endNumber = Math.Min(snapshot.LineCount - 1,
                    snapshot.GetLineNumberFromPosition(change.NewEnd) + LineMatcher.MaxStatementLines);
                var endLine = snapshot.GetLineFromLineNumber(endNumber);
                ClassificationChanged?.Invoke(this,
                    new ClassificationChangedEventArgs(new SnapshotSpan(startLine.Start, endLine.EndIncludingLineBreak)));
            }
        }
    }
}
