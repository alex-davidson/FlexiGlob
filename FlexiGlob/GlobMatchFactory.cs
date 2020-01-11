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

        public IGlobMatch Start(Segment[] segments) => Start(segments, defaultCaseSensitive);

        public IGlobMatch Start(Segment[] segments, bool caseSensitive)
        {
            var evaluator = new MatchEvaluator(segments, caseSensitive);
            if (!segments.Any()) return RecursiveMatchContext.NoMatch;
            return new RecursiveMatchContext(evaluator, new[] { MatchState.Start });
        }
    }
}
