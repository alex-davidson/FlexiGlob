using System.Collections.Generic;
using System.Diagnostics;

namespace FlexiGlob.Matching
{
    internal class MatchEvaluator
    {
        private readonly Segment[] segments;
        private readonly bool caseSensitive;

        public MatchEvaluator(Segment[] segments, bool caseSensitive)
        {
            this.segments = segments;
            this.caseSensitive = caseSensitive;
        }

        public IEnumerable<MatchState> Evaluate(IEnumerable<MatchState> matchStates, string pathSegment)
        {
            var nextMatchStates = new List<MatchState>();
            foreach (var state in matchStates)
            {
                if (!state.CanContinue) continue;

                EvaluateMatches(nextMatchStates, pathSegment, state, state.NextSegmentIndex);
            }
            return nextMatchStates;
        }

        private void EvaluateMatches(List<MatchState> nextMatchStates, string pathSegment, MatchState state, int index)
        {
            if (index >= segments.Length) return;

            var segment = segments[index];

            var isComplete = segments.Length == index + 1;
            if (segment is WildcardMultiSegment)
            {
                // Match just this segment:
                nextMatchStates.Add(new MatchState(state, index + 1, isComplete, !isComplete, MatchedVariable.None));
                // Try to match the next segment too:
                nextMatchStates.Add(new MatchState(state, index, isComplete, true, MatchedVariable.None));
                // Maybe skip this segment and match the next one instead:
                EvaluateMatches(nextMatchStates, pathSegment, state, index + 1);
            }
            else
            {
                var match = segment.Match(pathSegment, caseSensitive);
                if (!match.Success) return;

                nextMatchStates.Add(new MatchState(state, index + 1, isComplete, !isComplete, match.Variables));
            }
        }

        public string GetChildPrefix(IEnumerable<MatchState> matchStates)
        {
            var prefixes = new List<string>();
            foreach (var state in matchStates)
            {
                if (!state.CanContinue) continue;

                Debug.Assert(state.NextSegmentIndex < segments.Length);
                var segment = segments[state.NextSegmentIndex];

                if (segment.Prefix == "") return "";
                prefixes.Add(segment.Prefix);
            }
            if (prefixes.Count == 0) return "";
            return Util.LongestCommonPrefix(prefixes, caseSensitive);
        }
    }
}
