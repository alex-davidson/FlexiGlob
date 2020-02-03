namespace FlexiGlob
{
    public static class GlobMatchFilterExtensions
    {
        public static bool Filter<T>(this IGlobMatchFilter filter, GlobMatchEnumerator.Match<T> match) => filter.Filter(match.Details);
        public static bool Filter<T>(this IGlobMatchFilter filter, MultiGlobMatchEnumerator.MultiMatch<T> match) => filter.Filter(match.Details[0]);
    }
}
