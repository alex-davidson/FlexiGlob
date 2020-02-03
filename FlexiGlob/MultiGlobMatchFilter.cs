using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob
{
    /// <summary>
    /// Filters glob matches using more globs.
    /// </summary>
    /// <remarks>
    /// Inclusions and exclusions are applied in order, as for MultiGlobMatchEnumerator.
    /// This is useful when enumerating matches based on one set of rules, then postfiltering
    /// the results using another set of rules.
    /// </remarks>
    public class MultiGlobMatchFilter : IGlobMatchFilter
    {
        private readonly bool caseSensitive;
        private readonly GlobRule[] rules = new GlobRule[0];

        public MultiGlobMatchFilter(bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
        }

        private MultiGlobMatchFilter(bool caseSensitive, IEnumerable<GlobRule> rules)
        {
            this.caseSensitive = caseSensitive;
            this.rules = rules.ToArray();
        }

        public MultiGlobMatchFilter Include(params Glob[] globs) =>
            new MultiGlobMatchFilter(caseSensitive, rules.Concat(globs.Select(g => CreateRule(g, false))));
        public MultiGlobMatchFilter Exclude(params Glob[] globs) =>
            new MultiGlobMatchFilter(caseSensitive, rules.Concat(globs.Select(g => CreateRule(g, true))));

        private GlobRule CreateRule(Glob glob, bool exclude) => new GlobRule { Matcher = new GlobMatcher(glob, caseSensitive), Exclude = exclude };

        private struct GlobRule
        {
            public GlobMatcher Matcher { get; set; }
            public bool Exclude { get; set; }
        }

        public bool Filter(GlobMatch match)
        {
            foreach (var rule in rules)
            {
                if (rule.Matcher.IsMatch(match.GetPathSegments())) return !rule.Exclude;
            }
            return false;
        }
    }
}
