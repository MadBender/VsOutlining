using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;


namespace VsOutlining.Outlining.JS
{
    [Export(typeof(ITaggerProvider))]
	[TagType(typeof(IOutliningRegionTag))]
	[ContentType("JavaScript")]	

	internal sealed class JSOutliningTaggerProvider : ITaggerProvider
	{
		[Import]
		IClassifierAggregatorService classifierAggregator;
        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{            
			IClassifier classifier = classifierAggregator.GetClassifier(buffer);

            return buffer.Properties.GetOrCreateSingletonProperty(
                () => new JSOutliningTagger(buffer, classifier) as ITagger<T>
            );
		} 
	}
}
