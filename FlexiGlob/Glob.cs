using System.Linq;

namespace FlexiGlob
{
    public class Glob
    {
        public static readonly Glob[] None = new Glob[0];

        public RootSegment? Root { get; }
        public Segment[] Segments { get; }

        public Glob(RootSegment? root, params Segment[] segments)
        {
            Root = root;
            Segments = segments;
        }

        public override string ToString() => $"Root: {Root?.ToString() ?? "(none)"} / Segments: {string.Join(" / ", Segments.AsEnumerable())}";
    }
}
