using System.Linq;
using System.Text.RegularExpressions;
using FlexiGlob.Matching;

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

        public override SegmentMatchResult Match(string candidate, bool caseSensitive)
        {
            var regex = GetRegex(caseSensitive);
            if (regex.GetGroupNames().Any())
            {
                var match = regex.Match(candidate);
                if (match.Success)
                {
                    if (match.Groups.Count == 1)
                    {
                        // Only group 0 (the entire regex) was matched.
                        return SegmentMatchResult.Match;
                    }
                    var variables = match.Groups.Where(g => regex.GroupNumberFromName(g.Name) != 0).Select(g => new MatchedVariable(g.Name, g.Value)).ToArray();
                    return SegmentMatchResult.MatchWithVariables(variables);
                }
            }
            else
            {
                if (regex.IsMatch(candidate)) return SegmentMatchResult.Match;
            }
            return SegmentMatchResult.NoMatch;
        }

        private Regex GetRegex(bool caseSensitive) => new Regex(Regex, RegexOptions.ExplicitCapture | (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));

        public override string ToString() => $"{Token} (as /{Regex}/, using prefix {Prefix})";
    }
}
