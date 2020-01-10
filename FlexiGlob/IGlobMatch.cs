using System.Collections.Generic;

namespace FlexiGlob
{
    public interface IGlobMatch
    {
        /// <summary>
        /// True if this represents a complete match of the entire glob.
        /// </summary>
        bool IsMatch { get; }
        /// <summary>
        /// True if this could potentially match a child of the current location.
        /// </summary>
        bool CanContinue { get; }
        /// <summary>
        /// Get values of variables matched by the glob.
        /// </summary>
        /// <returns></returns>
        IEnumerable<MatchedVariable> GetVariables();
        /// <summary>
        /// Attempt to match the specified child of our current location.
        /// </summary>
        IGlobMatch MatchChild(string segment);
        /// <summary>
        /// Returns the longest prefix which a child must have in order to match.
        /// </summary>
        string GetPrefixFilter();
    }
}
