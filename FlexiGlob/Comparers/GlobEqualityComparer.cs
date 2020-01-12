using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob.Comparers
{
    /// <summary>
    /// Compare two glob path segments for equality.
    /// </summary>
    public class GlobEqualityComparer : IEqualityComparer<Glob?>
    {
        public static readonly GlobEqualityComparer CaseSensitive = new GlobEqualityComparer(SegmentEqualityComparer.CaseSensitive);
        public static readonly GlobEqualityComparer CaseInsensitive = new GlobEqualityComparer(SegmentEqualityComparer.CaseInsensitive);

        private readonly SegmentEqualityComparer segmentComparer;

        private GlobEqualityComparer(SegmentEqualityComparer segmentComparer)
        {
            this.segmentComparer = segmentComparer;
        }

        public bool Equals(Glob? x, Glob? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.Segments.Length != y.Segments.Length) return false;
            if (!RootSegmentEqualityComparer.Instance.Equals(x.Root, y.Root)) return false;
            return x.Segments.SequenceEqual(y.Segments, segmentComparer);
        }

        public int GetHashCode(Glob? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            var hashCode = RootSegmentEqualityComparer.Instance.GetHashCode();
            foreach (var segment in obj.Segments)
            {
                hashCode = (hashCode * 397) ^ segmentComparer.GetHashCode(segment);
            }
            return hashCode;
        }
    }
}
