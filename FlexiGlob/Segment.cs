using FlexiGlob.Matching;

namespace FlexiGlob
{
    public abstract class Segment
    {
        public string Token { get; }

        protected Segment(string token)
        {
            Token = token;
        }

        public abstract SegmentMatchResult Match(string candidate, bool caseSensitive);
        public abstract string Prefix { get; }

        public override string ToString() => Token;
    }
}
