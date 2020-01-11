using System;

namespace FlexiGlob.Matching
{
    [Flags]
    internal enum MatchFlags
    {
        None                = 0,
        /// <summary>
        /// State represents a complete match of the entire glob.
        /// </summary>
        IsMatch             = 0b0001,
        /// <summary>
        /// State could potentially match a child of the current location.
        /// </summary>
        CanContinue         = 0b0010,
        /// <summary>
        /// Every child of the current location will match.
        /// </summary>
        MatchesAllChildren  = 0b0100,
    }
}
