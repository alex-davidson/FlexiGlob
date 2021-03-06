﻿using System.Collections.Generic;

namespace FlexiGlob.Matching
{
    internal class MatchState
    {
        public static readonly MatchState Start = new MatchState();

        private readonly MatchState? parent;
        private readonly string segment;
        private readonly MatchedVariable[] variables;

        private MatchState()
        {
            NextSegmentIndex = 0;
            Flags = MatchFlags.CanContinue;
            segment = "";
            variables = MatchedVariable.None;
        }

        public MatchState(MatchState parent, int nextSegmentIndex, MatchFlags flags, string segment, MatchedVariable[] variables)
        {
            NextSegmentIndex = nextSegmentIndex;
            Flags = flags;
            this.parent = parent;
            this.segment = segment;
            this.variables = variables;
        }

        public int NextSegmentIndex { get; }
        public MatchFlags Flags { get; }

        public IEnumerable<MatchedVariable> GetVariables()
        {
            MatchState? current = this;
            while (current != null)
            {
                foreach (var variable in current.variables) yield return variable;
                current = current.parent;
            }
        }

        public IEnumerable<string> GetAncestry()
        {
            MatchState? current = this;
            while (current != null)
            {
                if (current.segment != "") yield return current.segment;
                current = current.parent;
            }
        }
    }
}
