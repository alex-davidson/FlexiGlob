using System;
using System.Collections.Generic;
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
            caseSensitiveImpl = new Lazy<Impl>(() => new Impl(Regex, true));
            caseInsensitiveImpl = new Lazy<Impl>(() => new Impl(Regex, false));
        }

        public override SegmentMatchResult Match(string candidate, bool caseSensitive)
        {
            var impl = GetImpl(caseSensitive);
            if (impl.VariableNames != null)
            {
                var match = impl.Regex.Match(candidate);
                if (match.Success)
                {
                    if (match.Groups.Count == 1)
                    {
                        // Only group 0 (the entire regex) was matched.
                        return SegmentMatchResult.Match;
                    }
                    var variables = new List<MatchedVariable>(impl.VariableNames.Length);
                    foreach (var name in impl.VariableNames)
                    {
                        var group = match.Groups[name];
                        if (!group.Success) continue;
                        variables.Add(new MatchedVariable(name, group.Value));
                    }
                    return SegmentMatchResult.MatchWithVariables(variables.ToArray());
                }
            }
            else
            {
                if (impl.Regex.IsMatch(candidate)) return SegmentMatchResult.Match;
            }
            return SegmentMatchResult.NoMatch;
        }

        private readonly Lazy<Impl> caseSensitiveImpl;
        private readonly Lazy<Impl> caseInsensitiveImpl;

        private Impl GetImpl(bool caseSensitive) => caseSensitive ? caseSensitiveImpl.Value : caseInsensitiveImpl.Value;

        public override string ToString() => $"{Token} (as /{Regex}/, using prefix {Prefix})";

        private struct Impl
        {
            public Impl(string regexString, bool caseSensitive)
            {
                Regex = new Regex(regexString, RegexOptions.ExplicitCapture | (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
                var names = new List<string>();
                foreach (var name in Regex.GetGroupNames())
                {
                    if (Regex.GroupNumberFromName(name) == 0) continue;
                    names.Add(name);
                }
                VariableNames = names.Any() ? names.ToArray() : null;
            }

            public string[]? VariableNames { get; }
            public Regex Regex { get; }
        }
    }
}
