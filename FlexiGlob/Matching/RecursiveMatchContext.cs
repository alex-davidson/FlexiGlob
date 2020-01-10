using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob.Matching
{
    internal class RecursiveMatchContext : IGlobMatch
    {
        private readonly MatchEvaluator evaluator;
        private readonly MatchState[] matchStates;

        public RecursiveMatchContext(MatchEvaluator evaluator, MatchState[] matchStates)
        {
            this.evaluator = evaluator;
            this.matchStates = matchStates;
            IsMatch = matchStates.Any(m => m.IsComplete);
            CanContinue = matchStates.Any(m => m.CanContinue);
        }

        public bool IsMatch { get; }
        public bool CanContinue { get; }

        public IGlobMatch MatchChild(string segment)
        {
            if (!matchStates.Any()) return NoMatch.Instance;

            var newMatchStates = evaluator.Evaluate(matchStates, segment).ToArray();
            if (!newMatchStates.Any()) return NoMatch.Instance;
            return new RecursiveMatchContext(evaluator, newMatchStates);
        }

        public string GetPrefixFilter() => evaluator.GetChildPrefix(matchStates);

        public IEnumerable<MatchedVariable> GetVariables()
        {
            if (!IsMatch) throw new InvalidOperationException("Match is not complete.");
            return matchStates.First(m => m.IsComplete).GetVariables();
        }
    }
}
