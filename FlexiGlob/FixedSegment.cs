using System;
using FlexiGlob.Matching;

namespace FlexiGlob
{
    public sealed class FixedSegment : Segment
    {
        public override string Prefix => Token;

        public FixedSegment(string token) : base(token)
        {
        }

        public override SegmentMatchResult Match(string candidate, bool caseSensitive)
        {
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            return comparer.Equals(candidate, Token) ? SegmentMatchResult.Match : SegmentMatchResult.NoMatch;
        }
    }
}
