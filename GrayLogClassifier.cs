//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace GrayLog
{
    /// <summary>
    /// GrayLog tag classifier finds every instance of GrayLogTag within a given span.
    /// </summary>
    internal class GrayLogClassifier : IClassifier
    {
        private readonly IClassificationType _classificationType;
        private readonly ITagAggregator<GrayLogTag> _tagger;

        internal GrayLogClassifier(ITagAggregator<GrayLogTag> tagger, IClassificationType GrayLogType)
        {
            _tagger = tagger;
            _classificationType = GrayLogType;
            
        }

        /// <summary>
        /// Get every GrayLogTag instance within the given span. Generally, the span in 
        /// question is the displayed portion of the file currently open in the Editor
        /// </summary>
        /// <param name="span">The span of text that will be searched for GrayLog tags</param>
        /// <returns>A list of every relevant tag in the given span</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var tags = _tagger.GetTags(span);

            return tags
                .Select(tagSpan => tagSpan.Span.GetSpans(span.Snapshot).First())
                .Select(GrayLogSpan => new ClassificationSpan(GrayLogSpan, _classificationType))
                .ToList();
        }

        /// <summary>
        /// Create an event for when the Classification changes
        /// </summary>
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add {}
            remove {}
        }
    }
}
