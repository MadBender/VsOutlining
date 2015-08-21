using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;


namespace VsOutlining.Outlining.CS
{
    [Export(typeof(ITaggerProvider))]
	[TagType(typeof(IOutliningRegionTag))]
	[ContentType("CSharp")]
    [ContentType("Razor.C#")]	

	internal sealed class CSOutliningTaggerProvider : ITaggerProvider
	{
		[Import]
		IClassifierAggregatorService classifierAggregator;
        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			IClassifier classifier = classifierAggregator.GetClassifier(buffer);
			
            var res = buffer.Properties.GetOrCreateSingletonProperty(
                () => new CSOutliningTagger(buffer, classifier) as ITagger<T>                    
            );
            return res;
		} 
	}
}
