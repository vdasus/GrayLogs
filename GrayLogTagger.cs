﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace GrayLog
{
    /// <summary>
    /// Empty GrayLogTag class.
    /// </summary>
    internal class GrayLogTag : IGlyphTag { }

    /// <summary>
    /// This class implements ITagger for GrayLogTag.  It is responsible for creating
    /// GrayLogTag TagSpans, which our GlyphFactory will then create glyphs for.
    /// </summary>
    internal class GrayLogTagger : ITagger<GrayLogTag>
    {
        private const string SEARCH_TEXT = "log.,logger.,log?.,logger?.";

        private static readonly string[] TagsList = SEARCH_TEXT.Split(',');
        
        /// <summary>
        /// This method creates GrayLogTag TagSpans over a set of SnapshotSpans.
        /// </summary>
        /// <param name="spans">A set of spans we want to get tags for.</param>
        /// <returns>The list of GrayLogTag TagSpans.</returns>
        IEnumerable<ITagSpan<GrayLogTag>> ITagger<GrayLogTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //GrayLog: implement tagging
            return from curSpan in spans
                where GetSearchTextPos(curSpan) > -1
                select new SnapshotSpan(curSpan.Snapshot, new Span(curSpan.Start, curSpan.Length))
                into grayLogSpan
                select new TagSpan<GrayLogTag>(grayLogSpan, new GrayLogTag());
        }

        private static int GetSearchTextPos(SnapshotSpan span)
        {
            foreach (var str in TagsList)
            {
                var rez = span.GetText().ToLower().IndexOf(str, StringComparison.Ordinal);
                if (rez > -1) return rez;
            }
            return -1;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add {}
            remove {}
        }
    }
}
