using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining.CS
{
    internal class CSOutliner: BaseOutliner
	{
		/// <summary>
		/// parses input buffer, searches for region start
		/// </summary>
		/// <param name="parser"></param>
		/// <returns>created region or null</returns>
		public override TextRegion TryCreateRegion(SnapshotParser parser)
		{
            ClassificationSpan span = parser.CurrentSpan;
            SnapshotPoint point = parser.CurrentPoint;
            if (span != null) {
                switch (span.ClassificationType.Classification) {
                    case "punctuation":
                        char c = point.GetChar();
                        switch (c) {
                            case '{':
                                return new TextRegion(span.Span.Start, TextRegionType.Block);                            
                            case '[':
                                //todo does it make sense in C#?
                                return new TextRegion(span.Span.Start, TextRegionType.Array);
                        }
                        break;
                    case "comment":
                        return ParseComment(parser);
                }
            }
            return null;
        }
        
		/// <summary>
		/// tries to close region
		/// </summary>
		/// <param name="parser">parser</param>
		/// <returns>whether region was closed</returns>
		protected override bool TryComplete(TextRegion r, SnapshotParser parser)
		{
            ClassificationSpan span = parser.CurrentSpan;
            SnapshotPoint point = parser.CurrentPoint;
            if (span != null) {                
                if (span.ClassificationType.Classification == "punctuation") {
                    char c = point.GetChar();
                    //text can be "};", not just "}"
                    if (r.RegionType == TextRegionType.Block && c == '}'
                        || r.RegionType == TextRegionType.Array && c == ']') {
                        r.EndPoint = span.Span.Start + 1;
                    }
                }
            }
            return r.Complete;
        }

        protected TextRegion ParseComment(SnapshotParser parser)
        {
            SnapshotPoint point = parser.CurrentPoint;
            ClassificationSpan span = parser.CurrentSpan;            
            return new TextRegion(point, TextRegionType.Comment) {
                EndPoint = span.Span.End
            };            
        }
    }
}