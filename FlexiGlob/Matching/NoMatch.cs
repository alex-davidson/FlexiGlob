using System;
using System.Collections.Generic;

namespace FlexiGlob.Matching
{
    internal class NoMatch : IGlobMatch
    {
        public static readonly NoMatch Instance = new NoMatch();

        public bool IsMatch => false;
        public bool CanContinue => false;
        public IEnumerable<MatchedVariable> GetVariables() => throw new InvalidOperationException("Match is not complete.");

        public IGlobMatch MatchChild(string segment) => this;
        public string GetPrefixFilter() => "";
    }
}
