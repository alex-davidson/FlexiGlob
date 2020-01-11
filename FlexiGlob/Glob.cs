namespace FlexiGlob
{
    public class Glob
    {
        public static readonly Glob[] None = new Glob[0];

        public RootSegment? Root { get; }
        public Segment[] Segments { get; }

        public Glob(RootSegment? root, Segment[] segments)
        {
            Root = root;
            Segments = segments;
        }
    }
}
