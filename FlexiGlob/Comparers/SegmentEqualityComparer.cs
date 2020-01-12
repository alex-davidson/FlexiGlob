using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob.Comparers
{
    /// <summary>
    /// Compare two glob path segments for equality.
    /// </summary>
    public class SegmentEqualityComparer :
        IEqualityComparer<Segment?>,
        IEqualityComparer<FixedSegment?>,
        IEqualityComparer<WildcardMultiSegment?>,
        IEqualityComparer<WildcardSegment?>
    {
        public static readonly SegmentEqualityComparer CaseSensitive = new SegmentEqualityComparer(StringComparer.Ordinal, true);
        public static readonly SegmentEqualityComparer CaseInsensitive = new SegmentEqualityComparer(StringComparer.OrdinalIgnoreCase, false);

        private readonly StringComparer stringComparer;
        private readonly bool caseSensitive;

        private SegmentEqualityComparer(StringComparer stringComparer, bool caseSensitive)
        {
            this.stringComparer = stringComparer;
            this.caseSensitive = caseSensitive;
        }

        public bool Equals(Segment? x, Segment? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            switch (x)
            {
                case FixedSegment fixedSegment: return Equals(fixedSegment, y as FixedSegment);
                case WildcardMultiSegment wildcardMultiSegment: return Equals(wildcardMultiSegment, y as WildcardMultiSegment);
                case WildcardSegment wildcardSegment: return Equals(wildcardSegment, y as WildcardSegment);
                default: return false;
            }
        }

        public bool Equals(FixedSegment? a, FixedSegment? b) => stringComparer.Equals(a?.Token, b?.Token);
        public bool Equals(WildcardMultiSegment? a, WildcardMultiSegment? b) => a?.GetType() == b?.GetType();
        public bool Equals(WildcardSegment? a, WildcardSegment? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null)) return false;
            if (ReferenceEquals(b, null)) return false;
            if (!stringComparer.Equals(a.Prefix, b.Prefix)) return false;
            if (!stringComparer.Equals(a.Regex, b.Regex)) return false;
            if (caseSensitive) return true; // Optimisation: variable names are part of the regex.
            return VariableNamesEqual(a.GetVariableNamesDirect(), b.GetVariableNamesDirect());
        }

        private static bool VariableNamesEqual(string[]? aVars, string[]? bVars)
        {
            if (ReferenceEquals(aVars, bVars)) return true;
            if (ReferenceEquals(aVars, null)) return false;
            if (ReferenceEquals(bVars, null)) return false;
            if (!Equals(aVars.Length, bVars.Length)) return false;
            // Variable names always compare case-sensitively.
            return aVars.SequenceEqual(bVars, StringComparer.Ordinal);
        }

        public int GetHashCode(Segment? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            switch (obj)
            {
                case FixedSegment fixedSegment: return GetHashCode(fixedSegment);
                case WildcardMultiSegment wildcardMultiSegment: return GetHashCode(wildcardMultiSegment);
                case WildcardSegment wildcardSegment: return GetHashCode(wildcardSegment);
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }

        public int GetHashCode(FixedSegment? obj) => stringComparer.GetHashCode(obj?.Token);
        public int GetHashCode(WildcardMultiSegment? obj) => obj?.GetType().GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
        public int GetHashCode(WildcardSegment? obj) => stringComparer.GetHashCode(obj?.Regex);
    }
}
