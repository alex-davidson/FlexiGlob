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

        public GlobMatch Start(Segment[] segments) => Start(segments, defaultCaseSensitive);

        public GlobMatch Start(Segment[] segments, bool caseSensitive)
        {
            var evaluator = new MatchEvaluator(segments, caseSensitive);
            if (!segments.Any()) return GlobMatch.NoMatch;
            return new GlobMatch(evaluator, new[] { MatchState.Start });
        }
    }
}
