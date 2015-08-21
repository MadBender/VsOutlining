using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VsOutlining.Outlining
{
	/// <summary>
	/// sequential parser for ITextSnapshot
	/// </summary>
	internal class SnapshotParser
	{
		protected ITextSnapshot Snapshot;
		public SnapshotPoint CurrentPoint { get; protected set; }
		//public ITextSnapshotLine CurrentLine { get { return CurrentPoint.GetContainingLine(); } }
		//classifier
		protected IClassifier Classifier;
		protected IList<ClassificationSpan> ClassificationSpans;
		/// <summary>
		/// A dictionary (span start => span)
		/// </summary>
		protected Dictionary<int, ClassificationSpan> SpanIndex = new Dictionary<int, ClassificationSpan>();
				
		public ClassificationSpan CurrentSpan {get; protected set;}		

		public SnapshotParser(ITextSnapshot snapshot, IClassifier classifier)
		{
			Snapshot = snapshot;
			Classifier = classifier;
			ClassificationSpans = Classifier.GetClassificationSpans(new SnapshotSpan(Snapshot, 0, snapshot.Length));
			foreach (ClassificationSpan s in ClassificationSpans) {
				SpanIndex.Add(s.Span.Start.Position, s);
            }
			CurrentPoint = Snapshot.GetLineFromLineNumber(0).Start;
            if (SpanIndex.ContainsKey(0)) {
                CurrentSpan = SpanIndex[0];
            }
		}
        
        /// <summary>
		/// Moves forward by one char or one classification span
		/// </summary>
		/// <returns>true, if moved</returns>
		public bool MoveNext()
        {
            if (!AtEnd()) {
                //operators are processed char by char, because the classifier can merge several operators into one span (like "]]", "[]")
                CurrentPoint = CurrentSpan != null 
                        && CurrentSpan.ClassificationType.Classification != "operator"
                        && CurrentSpan.ClassificationType.Classification != "punctuation"
                    ? CurrentSpan.Span.End 
                    : CurrentPoint + 1;

                if (SpanIndex.ContainsKey(CurrentPoint.Position)) {
                    CurrentSpan = SpanIndex[CurrentPoint.Position];
                } else if (CurrentSpan != null && CurrentPoint.Position >= CurrentSpan.Span.End.Position) {
                    //we're out of current span
                    CurrentSpan = null;
                }
                return true;
            }
            return false;
        }



        public bool AtEnd()
		{
			return CurrentPoint.Position >= Snapshot.Length;
		}
	}
}
