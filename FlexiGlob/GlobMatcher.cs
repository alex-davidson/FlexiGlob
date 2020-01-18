namespace FlexiGlob
{
    /// <summary>
    /// Matches individual paths against a glob's relative component.
    /// </summary>
    public class GlobMatcher
    {
        private readonly Glob glob;
        private readonly bool caseSensitive;

        public GlobMatcher(Glob glob, bool caseSensitive)
        {
            this.glob = glob;
            this.caseSensitive = caseSensitive;
        }

        public GlobMatch Match(params string[] pathSegments)
        {
            var state = new GlobMatchFactory(caseSensitive).Start(glob);
            foreach (var pathSegment in pathSegments)
            {
                if (!state.CanContinue) return GlobMatch.NoMatch;
                state = state.MatchChild(pathSegment);
            }
            return state;
        }

        public bool IsMatch(params string[] pathSegments) => Match(pathSegments).IsMatch;
    }
}
