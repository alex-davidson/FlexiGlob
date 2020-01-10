namespace FlexiGlob
{
    public class WildcardMultiSegment : Segment
    {
        public override string Prefix => "";

        public WildcardMultiSegment() : base("**")
        {
        }
    }
}
