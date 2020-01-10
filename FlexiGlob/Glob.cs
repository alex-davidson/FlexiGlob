namespace FlexiGlob
{
    public class Glob
    {
        public RootSegment? Root { get; }
        public Segment[] Segments { get; }

        public Glob(RootSegment? root, Segment[] segments)
        {
            Root = root;
            Segments = segments;
        }
    }
}
