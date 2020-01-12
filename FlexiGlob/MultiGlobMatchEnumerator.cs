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
        private readonly Glob[] includes;
        private readonly Glob[] excludes;

        public MultiGlobMatchEnumerator(Glob[] include, Glob[] exclude)
        {
            this.includes = include;
            this.excludes = exclude;
        }

        public IEnumerable<MultiMatch<T>> EnumerateMatches<T>(IGlobMatchableHierarchy<T> hierarchy)
        {
            var factory = new GlobMatchFactory(hierarchy.CaseSensitive);

            var startIncludes = includes.Select(factory.Start).ToArray();
            var startExcludes = excludes.Select(factory.Start).ToArray();
            var worklist = new Worklist<State<T>>();
            worklist.Add(new State<T>(hierarchy.Root, startIncludes, startExcludes));

            var newExcludes = new List<GlobMatch>();
            var newIncludes = new List<GlobMatch>();
            var matches = new List<GlobMatch>();

            while (worklist.TryTake(out var pair))
            {
                foreach (var child in hierarchy.GetChildrenMatchingPrefix(pair.Item, GetCommonPrefix(pair.Include)))
                {
                    var name = hierarchy.GetName(child);
                    var isContainer = hierarchy.IsContainer(child);
                    var isExcluded = false;
                    var isEntireSubtreeExcluded = false;
                    newExcludes.Clear();
                    newIncludes.Clear();
                    matches.Clear();
                    foreach (var exclude in pair.Exclude)
                    {
                        var newState = exclude.MatchChild(name);
                        if (newState.IsMatch) isExcluded = true;
                        if (newState.MatchesAllChildren)
                        {
                            isEntireSubtreeExcluded = true;
                            break;
                        }
                        if (newState.CanContinue && isContainer)
                        {
                            newExcludes.Add(newState);
                        }
                    }
                    if (isEntireSubtreeExcluded) continue;  // Early exit: a recursive wildcard excludes this entire subtree.
                    foreach (var include in pair.Include)
                    {
                        var newState = include.MatchChild(name);
                        if (!isExcluded && newState.IsMatch) matches.Add(newState);
                        if (newState.CanContinue && isContainer)
                        {
                            newIncludes.Add(newState);
                        }
                    }
                    if (matches.Any())
                    {
                        yield return new MultiMatch<T>(child, matches.ToArray());
                    }
                    if (newIncludes.Any())
                    {
                        worklist.Add(new State<T>(child, newIncludes.ToArray(), newExcludes.ToArray()));
                    }
                }
            }

            string GetCommonPrefix(GlobMatch[] states)
            {
                var prefixes = states.Select(s => s.GetPrefixFilter()).ToList();
                return Util.LongestCommonPrefix(prefixes, hierarchy.CaseSensitive);
            }
        }

        private struct State<T>
        {
            public T Item { get; }
            public GlobMatch[] Include { get; }
            public GlobMatch[] Exclude { get; }

            public State(T item, GlobMatch[] include, GlobMatch[] exclude)
            {
                Item = item;
                Include = include;
                Exclude = exclude;
            }
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
