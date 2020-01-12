using System;
using System.Runtime.CompilerServices;

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

    internal static class MatchFlagsExtensions
    {
        /// <summary>
        /// Eliminate costly boxing and method calls in the matching loop.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasMatchFlag(this MatchFlags flags, MatchFlags flag) => (flags & flag) != 0;
    }
}
