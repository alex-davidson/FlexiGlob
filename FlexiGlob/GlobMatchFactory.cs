using System.Linq;
using FlexiGlob.Matching;

namespace FlexiGlob
{
    public class GlobMatchFactory
    {
        private readonly bool defaultCaseSensitive;

        public GlobMatchFactory(bool defaultCaseSensitive)
        {
            this.defaultCaseSensitive = defaultCaseSensitive;
        }

        public GlobMatch Start(Glob glob) => Start(glob, defaultCaseSensitive);

        public GlobMatch Start(Glob glob, bool caseSensitive)
        {
            var evaluator = new MatchEvaluator(glob, caseSensitive);
            if (!glob.Segments.Any()) return GlobMatch.NoMatch;
            return new GlobMatch(evaluator, new[] { MatchState.Start });
        }
    }
}
