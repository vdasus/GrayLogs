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

namespace TodoClassification
{
    /// <summary>
    /// ToDo tag classifier finds every instance of ToDoTag within a given span.
    /// </summary>
    class ToDoClassifier : IClassifier
    {
        private readonly IClassificationType _classificationType;
        private readonly ITagAggregator<ToDoTag> _tagger;

        internal ToDoClassifier(ITagAggregator<ToDoTag> tagger, IClassificationType todoType)
        {
            _tagger = tagger;
            _classificationType = todoType;
        }

        /// <summary>
        /// Get every ToDoTag instance within the given span. Generally, the span in 
        /// question is the displayed portion of the file currently open in the Editor
        /// </summary>
        /// <param name="span">The span of text that will be searched for ToDo tags</param>
        /// <returns>A list of every relevant tag in the given span</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var tags = _tagger.GetTags(span);

            return tags
                .Select(tagSpan => tagSpan.Span.GetSpans(span.Snapshot).First())
                .Select(todoSpan => new ClassificationSpan(todoSpan, _classificationType))
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
