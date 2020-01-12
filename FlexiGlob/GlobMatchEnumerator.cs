using System.Collections.Generic;

namespace FlexiGlob
{
    /// <summary>
    /// Match a hierarchy against a glob.
    /// </summary>
    /// <remarks>
    /// Note that the starting point is supplied by the hierarchy, not the glob. Only the path segments
    /// of the glob are considered; the root is ignored.
    /// </remarks>
    public class GlobMatchEnumerator
    {
        private readonly Glob glob;

        public GlobMatchEnumerator(Glob glob)
        {
            this.glob = glob;
        }

        public IEnumerable<Match<T>> EnumerateMatches<T>(IGlobMatchableHierarchy<T> hierarchy)
        {
            var start = new GlobMatchFactory(hierarchy.CaseSensitive).Start(glob);
            var worklist = new Worklist<Match<T>>();
            worklist.Add(new Match<T>(hierarchy.Root, start));

            while (worklist.TryTake(out var pair))
            {
                foreach (var child in hierarchy.GetChildrenMatchingPrefix(pair.Item, pair.Details.GetPrefixFilter()))
                {
                    var newState = pair.Details.MatchChild(hierarchy.GetName(child));
                    if (newState.IsMatch) yield return new Match<T>(child, newState);
                    if (newState.CanContinue && hierarchy.IsContainer(child))
                    {
                        worklist.Add(new Match<T>(child, newState));
                    }
                }
            }
        }

        public struct Match<T>
        {
            public Match(T item, GlobMatch details)
            {
                Item = item;
                Details = details;
            }

            public T Item { get; }
            public GlobMatch Details { get; }
        }
    }
}
