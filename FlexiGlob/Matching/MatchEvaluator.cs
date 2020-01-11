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
                if (!state.Flags.HasFlag(MatchFlags.CanContinue)) continue;

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
                var flags = MatchFlags.CanContinue;
                if (isComplete) flags |= MatchFlags.IsMatch | MatchFlags.MatchesAllChildren;
                // Match this segment without consuming it:
                nextMatchStates.Add(new MatchState(state, index, flags, MatchedVariable.None));
                // Maybe skip this segment and match the next one instead:
                EvaluateMatches(nextMatchStates, pathSegment, state, index + 1);
            }
            else
            {
                var match = segment.Match(pathSegment, caseSensitive);
                if (!match.Success) return;

                var flags = isComplete ? MatchFlags.IsMatch : MatchFlags.CanContinue;
                nextMatchStates.Add(new MatchState(state, index + 1, flags, match.Variables));
            }
        }

        public string GetChildPrefix(IEnumerable<MatchState> matchStates)
        {
            var prefixes = new List<string>();
            foreach (var state in matchStates)
            {
                if (!state.Flags.HasFlag(MatchFlags.CanContinue)) continue;

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
