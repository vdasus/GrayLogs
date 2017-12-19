using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace TodoClassification
{
    /// <summary>
    /// Export a <see cref="IClassifierProvider"/>
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    internal class ToDoClassifierProvider : IClassifierProvider
    {
#pragma warning disable 0649 //"warning CS0649 : field is never assigned to"
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("todo")]
        internal ClassificationTypeDefinition ToDoClassificationType;

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;
#pragma warning restore 0649

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            IClassificationType classificationType = ClassificationRegistry.GetClassificationType("todo");

            var tagAggregator = TagAggregatorFactory.CreateTagAggregator<ToDoTag>(buffer);
            return new ToDoClassifier(tagAggregator, classificationType);
        }
    }
}