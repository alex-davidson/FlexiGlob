using FlexiGlob.Matching;

namespace FlexiGlob
{
    public sealed class WildcardMultiSegment : Segment
    {
        public override string Prefix => "";

        public WildcardMultiSegment() : base("**")
        {
        }

        public override SegmentMatchResult Match(string candidate, bool caseSensitive) => SegmentMatchResult.Match;
    }
}
