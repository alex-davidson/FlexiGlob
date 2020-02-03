using System.Collections.Generic;

namespace FlexiGlob
{
    /// <summary>
    /// Matches individual paths against a glob's relative component.
    /// </summary>
    public class GlobMatcher
    {
        private readonly GlobMatch start;

        public GlobMatcher(Glob glob, bool caseSensitive)
        {
            start = new GlobMatchFactory(caseSensitive).Start(glob);
        }

        public GlobMatch Match(IEnumerable<string> pathSegments)
        {
            var state = start;
            foreach (var pathSegment in pathSegments)
            {
                if (!state.CanContinue) return GlobMatch.NoMatch;
                state = state.MatchChild(pathSegment);
            }
            return state;
        }

        public bool IsMatch(IEnumerable<string> pathSegments) => Match(pathSegments).IsMatch;
    }
}
