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
        private readonly Segment[] segments;

        public GlobMatchEnumerator(Glob glob) : this(glob.Segments)
        {
        }

        public GlobMatchEnumerator(Segment[] segments)
        {
            this.segments = segments;
        }

        public IEnumerable<Match<T>> EnumerateMatches<T>(IGlobMatchableHierarchy<T> hierarchy)
        {
            var start = new GlobMatchFactory(hierarchy.CaseSensitive).Start(segments);
            var queue = new Queue<Match<T>>();
            queue.Enqueue(new Match<T>(hierarchy.Root, start));

            while (queue.TryDequeue(out var pair))
            {
                foreach (var child in hierarchy.GetChildrenMatchingPrefix(pair.Item, pair.Details.GetPrefixFilter()))
                {
                    var newState = pair.Details.MatchChild(hierarchy.GetName(child));
                    if (newState.IsMatch) yield return new Match<T>(child, newState);
                    if (newState.CanContinue && hierarchy.IsContainer(child))
                    {
                        queue.Enqueue(new Match<T>(child, newState));
                    }
                }
            }
        }

        public struct Match<T>
        {
            public Match(T item, IGlobMatch details)
            {
                Item = item;
                Details = details;
            }

            public T Item { get; }
            public IGlobMatch Details { get; }
        }
    }
}
