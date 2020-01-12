using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FlexiGlob.Matching;

namespace FlexiGlob
{
    public sealed class WildcardSegment : Segment
    {
        public string Regex { get; }
        public override string Prefix { get; }

        public WildcardSegment(string token, string regex, string prefix = "") : base(token)
        {
            this.Regex = regex;
            Prefix = prefix;
            impl = new Impl(regex);
        }

        public override SegmentMatchResult Match(string candidate, bool caseSensitive)
        {
            if (Prefix.Length > candidate.Length) return SegmentMatchResult.NoMatch;
            if (!candidate.StartsWith(Prefix, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) return SegmentMatchResult.NoMatch;
            if (impl.VariableNames == null)
            {
                return MatchOnly(candidate, caseSensitive);
            }
            return MatchWithCapture(candidate, caseSensitive);
        }

        private SegmentMatchResult MatchOnly(string candidate, bool caseSensitive)
        {
            var regex = caseSensitive ? impl.CaseSensitiveRegex : impl.CaseInsensitiveRegex;
            if (regex.IsMatch(candidate)) return SegmentMatchResult.Match;
            return SegmentMatchResult.NoMatch;
        }

        private SegmentMatchResult MatchWithCapture(string candidate, bool caseSensitive)
        {
            var regex = caseSensitive ? impl.CaseSensitiveRegex : impl.CaseInsensitiveRegex;
            var match = regex.Match(candidate);
            if (!match.Success) return SegmentMatchResult.NoMatch;
            if (match.Groups.Count == 1)
            {
                // Only group 0 (the entire regex) was matched.
                return SegmentMatchResult.Match;
            }

            var variables = new List<MatchedVariable>(impl.VariableNames!.Length);
            foreach (var name in impl.VariableNames)
            {
                var group = match.Groups[name];
                if (!@group.Success) continue;
                variables.Add(new MatchedVariable(name, @group.Value));
            }

            return SegmentMatchResult.MatchWithVariables(variables.ToArray());
        }

        private readonly Impl impl;

        public override string ToString() => $"{Token} (as /{Regex}/, using prefix {Prefix})";

        internal string[]? GetVariableNamesDirect() => impl.VariableNames;
        public IEnumerable<string> GetVariableNames() => impl.VariableNames.AsEnumerable() ?? Enumerable.Empty<string>();

        private struct Impl
        {
            public Regex CaseSensitiveRegex { get; }
            public Regex CaseInsensitiveRegex { get; }
            public string[]? VariableNames { get; }

            public Impl(string regexString)
            {
                CaseSensitiveRegex = new Regex(regexString, RegexOptions.ExplicitCapture);
                CaseInsensitiveRegex = new Regex(regexString, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                var names = new List<string>();
                foreach (var name in CaseSensitiveRegex.GetGroupNames())
                {
                    if (CaseSensitiveRegex.GroupNumberFromName(name) == 0) continue;
                    names.Add(name);
                }
                VariableNames = names.Any() ? names.ToArray() : null;
            }
        }
    }
}
