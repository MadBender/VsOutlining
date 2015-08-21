using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining.CS
{
	internal class CSOutliningTagger: BaseOutliningTagger
	{
		public CSOutliningTagger(ITextBuffer buffer, IClassifier classifier)
			: base(buffer, classifier)
		{            
		}

		protected override void Init()
		{
			Outliner = new CSOutliner();
			base.Init();
		}

        protected override List<TextRegion> GetRegionList(TextRegion tree)
        {
            //Visual Studio outlines functions itself, let's not conflict with it
            return VsVersion.Major < 14
                ? base.GetRegionList(tree).FindAll(r => r.RegionSubType != TextRegionSubType.Function)
                //VS 2015 outlines blocks and arrays on its own  
                //todo does it make sense for C#?
                : base.GetRegionList(tree).FindAll(r => r.RegionType != TextRegionType.Array);
        }        
    }
}
