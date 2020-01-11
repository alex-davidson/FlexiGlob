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

            var startIncludes = includes.Select(i => new Matched(i, factory.Start(i.Segments))).ToArray();
            var startExcludes = excludes.Select(e => new Matched(e, factory.Start(e.Segments))).ToArray();
            var queue = new Queue<State<T>>();
            queue.Enqueue(new State<T>(hierarchy.Root, startIncludes, startExcludes));

            var newExcludes = new List<Matched>();
            var newIncludes = new List<Matched>();
            var matches = new List<Matched>();

            while (queue.TryDequeue(out var pair))
            {
                foreach (var child in hierarchy.GetChildrenMatchingPrefix(pair.Item, GetCommonPrefix(pair.Include)))
                {
                    var name = hierarchy.GetName(child);
                    var isContainer = hierarchy.IsContainer(child);
                    var isExcluded = false;
                    newExcludes.Clear();
                    newIncludes.Clear();
                    matches.Clear();
                    foreach (var exclude in pair.Exclude)
                    {
                        var newState = exclude.Details.MatchChild(name);
                        if (newState.IsMatch) isExcluded = true;
                        if (newState.CanContinue && isContainer)
                        {
                            newExcludes.Add(new Matched(exclude.Glob, newState));
                        }
                    }
                    foreach (var include in pair.Include)
                    {
                        var newState = include.Details.MatchChild(name);
                        if (!isExcluded && newState.IsMatch) matches.Add(new Matched(include.Glob, newState));
                        if (newState.CanContinue && isContainer)
                        {
                            newIncludes.Add(new Matched(include.Glob, newState));
                        }
                    }
                    if (matches.Any())
                    {
                        yield return new MultiMatch<T>(child, matches.ToArray());
                    }
                    if (newIncludes.Any())
                    {
                        queue.Enqueue(new State<T>(child, newIncludes.ToArray(), newExcludes.ToArray()));
                    }
                }
            }

            string GetCommonPrefix(Matched[] states)
            {
                var prefixes = states.Select(s => s.Details.GetPrefixFilter()).ToList();
                return Util.LongestCommonPrefix(prefixes, hierarchy.CaseSensitive);
            }
        }

        private struct State<T>
        {
            public T Item { get; }
            public Matched[] Include { get; }
            public Matched[] Exclude { get; }

            public State(T item, Matched[] include, Matched[] exclude)
            {
                Item = item;
                Include = include;
                Exclude = exclude;
            }
        }

        public struct MultiMatch<T>
        {
            public MultiMatch(T item, Matched[] sources)
            {
                Item = item;
                Sources = sources;
            }

            public T Item { get; }
            public Matched[] Sources { get; }
        }

        public struct Matched
        {
            public Matched(Glob glob, GlobMatch details)
            {
                Glob = glob;
                Details = details;
            }

            public Glob Glob { get; set; }
            public GlobMatch Details { get; }
        }
    }
}
