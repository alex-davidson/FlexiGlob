using System.Text.RegularExpressions;

namespace FlexiGlob
{
    public class WildcardSegment : Segment
    {
        public string Regex { get; }
        public override string Prefix { get; }

        public WildcardSegment(string token, string regex, string prefix = "") : base(token)
        {
            this.Regex = regex;
            Prefix = prefix;
        }

        public override string ToString() => $"{Token} (as /{Regex}/, using prefix {Prefix})";
    }
}
