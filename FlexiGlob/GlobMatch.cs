using System;
using System.Collections.Generic;
using System.Linq;
using FlexiGlob.Matching;

namespace FlexiGlob
{
    public sealed class GlobMatch
    {
        public static readonly GlobMatch NoMatch = new GlobMatch();

        private readonly MatchEvaluator? evaluator;
        private readonly MatchState[] matchStates;

        private GlobMatch()
        {
            flags = MatchFlags.None;
            matchStates = new MatchState[0];
            evaluator = null;
        }

        internal GlobMatch(MatchEvaluator evaluator, MatchState[] matchStates)
        {
            this.evaluator = evaluator;
            this.matchStates = matchStates;
            foreach (var state in matchStates)
            {
                flags |= state.Flags;
            }
        }

        private readonly MatchFlags flags;
        /// <summary>
        /// True if this represents a complete match of the entire glob.
        /// </summary>
        public bool IsMatch => flags.HasMatchFlag(MatchFlags.IsMatch);
        /// <summary>
        /// True if this could potentially match a child of the current location.
        /// </summary>
        public bool CanContinue => flags.HasMatchFlag(MatchFlags.CanContinue);
        /// <summary>
        /// True if every child, recursively, will match as well.
        /// </summary>
        public bool MatchesAllChildren => flags.HasMatchFlag(MatchFlags.MatchesAllChildren);

        /// <summary>
        /// Attempt to match the specified child of our current location.
        /// </summary>
        public GlobMatch MatchChild(string segment)
        {
            if (!matchStates.Any()) return NoMatch;

            var newMatchStates = evaluator!.Evaluate(matchStates, segment).ToArray();
            if (!newMatchStates.Any()) return NoMatch;
            return new GlobMatch(evaluator, newMatchStates);
        }

        /// <summary>
        /// Returns the longest prefix which a child must have in order to match.
        /// </summary>
        public string GetPrefixFilter() => evaluator?.GetChildPrefix(matchStates) ?? "";

        /// <summary>
        /// The glob to which this match relates.
        /// </summary>
        public Glob Glob => evaluator?.Glob ?? throw new InvalidOperationException("Match is not complete.");

        /// <summary>
        /// Get values of variables matched by the glob.
        /// </summary>
        public IEnumerable<MatchedVariable> GetVariables()
        {
            if (!IsMatch) throw new InvalidOperationException("Match is not complete.");
            return matchStates.First(m => m.Flags.HasMatchFlag(MatchFlags.IsMatch)).GetVariables();
        }
    }
}
