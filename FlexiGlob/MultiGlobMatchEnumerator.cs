using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob
{
    /// <summary>
    /// Match a hierarchy against multiple globs, applying inclusion and exclusion rules.
    /// </summary>
    /// <remarks>
    /// Note that the starting point is supplied by the hierarchy, not the globs. Only the path segments
    /// of the globs are considered; their roots are ignored.
    /// </remarks>
    public class MultiGlobMatchEnumerator
    {
        private readonly GlobRule[] rules = new GlobRule[0];

        public MultiGlobMatchEnumerator()
        {
        }

        private MultiGlobMatchEnumerator(IEnumerable<GlobRule> rules)
        {
            this.rules = rules.ToArray();
        }

        public MultiGlobMatchEnumerator Include(params Glob[] globs) =>
            new MultiGlobMatchEnumerator(rules.Concat(globs.Select(g => new GlobRule { Glob = g, Exclude = false })));
        public MultiGlobMatchEnumerator Exclude(params Glob[] globs) =>
            new MultiGlobMatchEnumerator(rules.Concat(globs.Select(g => new GlobRule { Glob = g, Exclude = true })));

        private struct GlobRule
        {
            public Glob Glob { get; set; }
            public bool Exclude { get; set; }
        }

        public IEnumerable<MultiMatch<T>> EnumerateMatches<T>(IGlobMatchableHierarchy<T> hierarchy)
        {
            var factory = new GlobMatchFactory(hierarchy.CaseSensitive);

            var starts = rules.Select(r => new Rule(factory.Start(r.Glob), r.Exclude)).ToArray();
            var worklist = new Worklist<State<T>>();
            worklist.Add(new State<T>(hierarchy.Root, starts));

            var newStates = new List<Rule>();
            var matches = new List<GlobMatch>();

            while (worklist.TryTake(out var pair))
            {
                var includePrefix = GetCommonPrefix(pair.Current.Where(c => !c.Exclude));
                if (includePrefix == null) continue;    // No inclusions remaining for this node.
                foreach (var child in hierarchy.GetChildrenMatchingPrefix(pair.Item, includePrefix))
                {
                    var name = hierarchy.GetName(child);
                    var isContainer = hierarchy.IsContainer(child);
                    var isExcluded = false;

                    newStates.Clear();
                    matches.Clear();
                    foreach (var state in pair.Current)
                    {
                        var newState = state.Details.MatchChild(name);
                        if (state.Exclude)
                        {
                            if (newState.IsMatch) isExcluded = true;
                            if (newState.MatchesAllChildren)
                            {
                                // Early exit: entire subtree is ignored for subsequent rules.
                                break;
                            }
                        }
                        else if (!isExcluded)
                        {
                            if (newState.IsMatch) matches.Add(newState);
                        }

                        if (newState.CanContinue && isContainer)
                        {
                            newStates.Add(new Rule(newState, state.Exclude));
                        }
                    }
                    if (matches.Any())
                    {
                        yield return new MultiMatch<T>(child, matches.ToArray());
                    }
                    if (newStates.Any())
                    {
                        worklist.Add(new State<T>(child, newStates.ToArray()));
                    }
                }
            }

            string? GetCommonPrefix(IEnumerable<Rule> states)
            {
                var prefixes = states.Select(s => s.Details.GetPrefixFilter()).ToList();
                if (prefixes.Count == 0) return null;
                return Util.LongestCommonPrefix(prefixes, hierarchy.CaseSensitive);
            }
        }

        private struct State<T>
        {
            public T Item { get; }
            public Rule[] Current { get; }

            public State(T item, Rule[] current)
            {
                Item = item;
                Current = current;
            }
        }

        private struct Rule
        {
            public Rule(GlobMatch details, bool exclude)
            {
                Details = details;
                Exclude = exclude;
            }

            public GlobMatch Details { get; }
            public bool Exclude { get; }
        }

        public struct MultiMatch<T>
        {
            public MultiMatch(T item, GlobMatch[] details)
            {
                Item = item;
                Details = details;
            }

            public T Item { get; }
            public GlobMatch[] Details { get; }
        }
    }
}
