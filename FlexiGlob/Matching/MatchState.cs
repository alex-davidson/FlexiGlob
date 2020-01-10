using System.Collections.Generic;

namespace FlexiGlob.Matching
{
    internal class MatchState
    {
        public static readonly MatchState Start = new MatchState();

        private readonly MatchState? parent;
        private readonly MatchedVariable[] variables;

        private MatchState()
        {
            NextSegmentIndex = 0;
            IsComplete = false;
            CanContinue = true;
            variables = MatchedVariable.None;
        }

        public MatchState(MatchState parent, int nextSegmentIndex, bool isComplete, bool canContinue, MatchedVariable[] variables)
        {
            NextSegmentIndex = nextSegmentIndex;
            IsComplete = isComplete;
            CanContinue = canContinue;
            this.parent = parent;
            this.variables = variables;
        }

        public int NextSegmentIndex { get; }
        public bool IsComplete { get; }
        public bool CanContinue { get; }

        public IEnumerable<MatchedVariable> GetVariables()
        {
            MatchState? current = this;
            while (current != null)
            {
                foreach (var variable in current.variables) yield return variable;
                current = current.parent;
            }
        }
    }
}
