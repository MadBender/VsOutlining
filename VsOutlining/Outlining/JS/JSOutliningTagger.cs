using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining.JS
{
	internal class JSOutliningTagger: BaseOutliningTagger
	{
		public JSOutliningTagger(ITextBuffer buffer, IClassifier classifier)
			: base(buffer, classifier)
		{            
		}

		protected override void Init()
		{
			Outliner = new JSOutliner();
			base.Init();
		}

        protected override List<TextRegion> GetRegionList(TextRegion tree)
        {
            //Visual Studio outlines functions itself, let's not conflict with it
            return VsVersion.Major < 14
                ? base.GetRegionList(tree).FindAll(r => r.RegionSubType != TextRegionSubType.Function)
                //VS 2015 outlines blocks and arrays on its own            
                : base.GetRegionList(tree).FindAll(r => r.RegionType != TextRegionType.Block && r.RegionType != TextRegionType.Array);
        }        
    }
}
