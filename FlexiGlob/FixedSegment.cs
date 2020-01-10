using System;

namespace FlexiGlob
{
    public class FixedSegment : Segment
    {
        public override string Prefix => Token;

        public FixedSegment(string token) : base(token)
        {
        }
    }
}
