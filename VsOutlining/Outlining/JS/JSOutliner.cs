using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining.JS
{
    internal class JSOutliner: BaseOutliner
	{
		private static Regex RegionStartRegex = new Regex(@"^\/[\/\*]\s*#region\s(.+?)(\*\/)?$", RegexOptions.Compiled);
		private static Regex RegionEndRegex = new Regex(@"^\/[\/\*]\s*#endregion", RegexOptions.Compiled);
        
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
                    case "operator":
                        char c = point.GetChar();
                        switch (c) {
                            case '{':
                                return new TextRegion(span.Span.Start, TextRegionType.Block);
                            case '[':
                                return new TextRegion(span.Span.Start, TextRegionType.Array);
                        }
                        break;
                    case "comment":
                        return ParseComment(parser, RegionStartRegex, RegionEndRegex);
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
                string text = span.Span.GetText();
                if (span.ClassificationType.Classification == "operator") {
                    char c = point.GetChar();
                    //text can be "};", not just "}"
                    if (r.RegionType == TextRegionType.Block && c == '}'
                        || r.RegionType == TextRegionType.Array && c == ']') {
                        r.EndPoint = span.Span.Start + 1;
                    }
                } else if (span.ClassificationType.Classification == "comment" && r.RegionType == TextRegionType.Region) {
                    Match m = RegionEndRegex.Match(text);
                    if (m.Success) {
                        r.EndPoint = span.Span.End;
                    }
                }
            }
            return r.Complete;
        }	


        protected TextRegion ParseComment(SnapshotParser parser, Regex regionStartRegex, Regex regionEndRegex)
        {
            SnapshotPoint point = parser.CurrentPoint;
            ClassificationSpan span = parser.CurrentSpan;
            Match m = regionStartRegex.Match(span.Span.GetText());
            if (m.Success) {
                return new TextRegion(point, TextRegionType.Region) {
                    Name = m.Groups[1].Value
                };
            }
            if (!regionEndRegex.IsMatch(span.Span.GetText())) {
                return new TextRegion(point, TextRegionType.Comment) {
                    EndPoint = span.Span.End
                };
            }
            return null;
        }       

    }
}