using System;
using System.Collections.Generic;

namespace FlexiGlob.Comparers
{
    public class RootSegmentEqualityComparer :
        IEqualityComparer<RootSegment?>,
        IEqualityComparer<LocalRootSegment?>,
        IEqualityComparer<UNCRootSegment?>
    {
        public static readonly RootSegmentEqualityComparer Instance = new RootSegmentEqualityComparer();

        /// <summary>
        /// Neither drive letters nor UNC machine names nor UNC share names are case-sensitive.
        /// </summary>
        private readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(RootSegment? x, RootSegment? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            switch (x)
            {
                case LocalRootSegment localRootSegment: return Equals(localRootSegment, y as LocalRootSegment);
                case UNCRootSegment uncRootSegment: return Equals(uncRootSegment, y as UNCRootSegment);
                default: return false;
            }
        }
        public bool Equals(LocalRootSegment? x, LocalRootSegment? y) => stringComparer.Equals(x?.Token, y?.Token);
        public bool Equals(UNCRootSegment? x, UNCRootSegment? y) => stringComparer.Equals(x?.Machine, y?.Machine) && stringComparer.Equals(x?.Share, y?.Share);

        public int GetHashCode(RootSegment? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            switch (obj)
            {
                case LocalRootSegment localRootSegment: return GetHashCode(localRootSegment);
                case UNCRootSegment uncRootSegment: return GetHashCode(uncRootSegment);
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }
        public int GetHashCode(LocalRootSegment? obj) => stringComparer.GetHashCode(obj?.Token);
        public int GetHashCode(UNCRootSegment? obj) => (stringComparer.GetHashCode(obj?.Machine) * 397) ^ stringComparer.GetHashCode(obj?.Share);
    }
}
