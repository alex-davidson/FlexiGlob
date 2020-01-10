using System.Collections.Generic;

namespace FlexiGlob
{
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
            var queue = new Queue<(T, IGlobMatch)>();
            queue.Enqueue((hierarchy.Root, start));

            while (queue.TryDequeue(out var pair))
            {
                var (item, state) = pair;

                foreach (var child in hierarchy.GetChildrenMatchingPrefix(item, state.GetPrefixFilter()))
                {
                    var newState = state.MatchChild(hierarchy.GetName(child));
                    if (newState.IsMatch) yield return new Match<T>(child, newState);
                    if (newState.CanContinue && hierarchy.IsContainer(child))
                    {
                        queue.Enqueue((child, newState));
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
