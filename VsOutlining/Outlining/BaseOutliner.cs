using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining
{
    /// <summary>
    /// common parts of C# and JS outliners
    /// </summary>
    internal abstract class BaseOutliner
	{
        protected bool FunctionOccured;

        /// <summary>
        /// parses input buffer, searches for region start
        /// </summary>
        /// <param name="parser"></param>
        /// <returns>created region or null</returns>
        public abstract TextRegion TryCreateRegion(SnapshotParser parser);
			

		/// <summary>
		/// tries to close region
		/// </summary>
		/// <param name="parser">parser</param>
		/// <returns>whether region was closed</returns>
		protected abstract bool TryComplete(TextRegion r, SnapshotParser parser);
		
		/// <summary>
		/// parser the text buffer
		/// </summary>
		/// <param name="parser">buffer parser</param>
		/// <returns>text region tree</returns>
		public TextRegion ParseBuffer(SnapshotParser parser)
		{
            FunctionOccured = false;
            TextRegion regionTree = new TextRegion();		
			while (ParseBuffer(parser, regionTree) != null);
			return regionTree;
		}

		/// <summary>
		/// parses buffer
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="parent">parent region or null</param>
		/// <returns>a region with its children or null</returns>
		protected virtual TextRegion ParseBuffer(SnapshotParser parser, TextRegion parent)
		{
            for (; !parser.AtEnd(); parser.MoveNext()) {
                ProcessCurrentToken(parser);
                TextRegion r = TryCreateRegion(parser);

                if (r != null) {
                    //found the start of the region
                    OnRegionFound(r);
                    r.Parent = parent;
                    parser.MoveNext();
                    if (!r.Complete) {
                        //searching for child regions						
                        while (ParseBuffer(parser, r) != null) ;
                    }
                    //adding to children or merging with last child
                    if (!TryMergeComments(parent, r)) {
                        parent.Children.Add(r);
                        ExtendStartPoint(r);
                    }
                    return r;
                }
                //found parent's end - terminating parsing
                if (TryComplete(parent, parser)) {
                    parser.MoveNext();
                    return null;
                }
            }
            return null;
        }

		/// <summary>
		/// A function that looks for special tokens in source code
		/// </summary>
        protected virtual void ProcessCurrentToken(SnapshotParser p)
        {         
            
        }

        protected void OnRegionFound(TextRegion r)
        {         
            if (r.RegionType == TextRegionType.Block && FunctionOccured) {
                r.RegionSubType = TextRegionSubType.Function;
            }
            FunctionOccured = false;
        }


        /// <summary>
        /// Tries to move region start point up to get C#-like outlining
        /// 
        /// for (var k in obj)
        /// { -- from here
        /// 
        /// for (var k in obj) -- to here
        /// {
        /// </summary>
        private void ExtendStartPoint(TextRegion r)
		{
			//some are not extended
			if (r.RegionType == TextRegionType.Region
				|| r.RegionType == TextRegionType.Comment
				|| !r.Complete
				|| r.StartLine.LineNumber == r.EndLine.LineNumber
				|| !string.IsNullOrWhiteSpace(r.TextBefore)) return;

            //how much can we move region start
            int upperLimit = 0;
            if (r.Parent != null) {
                int childPosition = r.Parent.Children.IndexOf(r);

                if (childPosition == -1)
                    childPosition = r.Parent.Children.Count;
                if (childPosition == 0) {
                    //this region is first child of its parent
                    //we can go until the parent's start
                    upperLimit = r.Parent.RegionType != TextRegionType.None ? r.Parent.StartLine.LineNumber + 1 : 0;
                } else {
                    //there is previous child
                    //we can go until its end
                    TextRegion prevRegion = r.Parent.Children[childPosition - 1];
                    upperLimit = prevRegion.EndLine.LineNumber + (prevRegion.EndLine.LineNumber == prevRegion.StartLine.LineNumber ? 0 : 1);
                }
            }

            //now looking up to calculated upper limit for non-empty line
            for (int i = r.StartLine.LineNumber - 1; i >= upperLimit; i--) {
                ITextSnapshotLine line = r.StartPoint.Snapshot.GetLineFromLineNumber(i);
                if (!string.IsNullOrWhiteSpace(line.GetText())) {
                    //found such line, placing region start at its end
                    r.StartPoint = line.End;
                    return;
                }
            }
        }

        /// <summary>
        /// tries to merge sequential comments		
        /// </summary>
        /// <returns>true, if merged. In this case newRegion is not added to Children</returns>
        protected virtual bool TryMergeComments(TextRegion r, TextRegion newRegion)
        {
            if (r.Children.Count > 0) {
                TextRegion last = r.Children[r.Children.Count - 1];
                //merge conditions
                if (last.RegionType == TextRegionType.Comment
                    && newRegion.RegionType == TextRegionType.Comment
                    && newRegion.StartLine.LineNumber <= last.EndLine.LineNumber + 1
                    && string.IsNullOrWhiteSpace(new SnapshotSpan(last.EndPoint, newRegion.StartPoint).GetText())) {
                    //instead of adding newRegion, we just move last child's end
                    last.EndPoint = newRegion.EndPoint;
                    return true;
                }
            }
            return false;
        }

	}
}