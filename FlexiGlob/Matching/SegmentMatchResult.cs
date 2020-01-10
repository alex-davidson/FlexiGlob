namespace FlexiGlob.Matching
{
    public class SegmentMatchResult
    {
        public static readonly SegmentMatchResult NoMatch = new SegmentMatchResult();
        public static readonly SegmentMatchResult Match = new SegmentMatchResult() { Success = true };
        public static SegmentMatchResult MatchWithVariables(MatchedVariable[] variables) => new SegmentMatchResult { Success = true, Variables =  variables };

        public bool Success { get; private set; }
        public MatchedVariable[] Variables { get; private set; } = MatchedVariable.None;
    }
}
