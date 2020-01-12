namespace FlexiGlob.Matching
{
    public struct SegmentMatchResult
    {
        public static readonly SegmentMatchResult NoMatch = new SegmentMatchResult { };
        public static readonly SegmentMatchResult Match = new SegmentMatchResult() { Variables = MatchedVariable.None };
        public static SegmentMatchResult MatchWithVariables(MatchedVariable[] variables) => new SegmentMatchResult { Variables = variables };

        public bool Success => Variables != null;
        public MatchedVariable[] Variables { get; private set; }
    }
}
