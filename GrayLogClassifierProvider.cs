using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace GrayLog
{
    /// <summary>
    /// Export a <see cref="IClassifierProvider"/>
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")]
    internal class GrayLogClassifierProvider : IClassifierProvider
    {
#pragma warning disable 0649 //"warning CS0649 : field is never assigned to"
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("GrayLog")]
        internal ClassificationTypeDefinition GrayLogClassificationType;

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;

        [Import]
        internal IBufferTagAggregatorFactoryService TagAggregatorFactory;
#pragma warning restore 0649

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            IClassificationType classificationType = ClassificationRegistry.GetClassificationType("GrayLog");

            var tagAggregator = TagAggregatorFactory.CreateTagAggregator<GrayLogTag>(buffer);
            return new GrayLogClassifier(tagAggregator, classificationType);
        }
    }
}