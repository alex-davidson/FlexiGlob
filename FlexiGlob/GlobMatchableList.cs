using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexiGlob
{
    /// <summary>
    /// Base class for a single-level hierarchy.
    /// </summary>
    /// <remarks>
    /// This requires a placeholder Root object from which the search can start. By default the
    /// default value of the type T will be used, which will mean null for reference types. If
    /// this is problematic for any reason, a placeholder can be specified via a constructor
    /// overload.
    /// </remarks>
    public abstract class GlobMatchableList<T> : IGlobMatchableHierarchy<T>
    {
        private readonly T[] items;

        protected GlobMatchableList(T[] items)
        {
            Root = default!;
            this.items = items;
        }

        protected GlobMatchableList(T rootPlaceholder, T[] items)
        {
            Root = rootPlaceholder;
            this.items = items;
        }

        public abstract string GetName(T item);
        /// <summary>
        /// Placeholder, notionally the 'parent' of all the items in the list.
        /// </summary>
        public T Root { get; }

        public bool IsContainer(T item) => Equals(Root, item);

        public IEnumerable<T> GetChildrenMatchingPrefix(T item, string prefix)
        {
            if (!IsContainer(item)) return Enumerable.Empty<T>();
            var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return items.Where(i => GetName(i).StartsWith(prefix, comparison));
        }

        public bool CaseSensitive { get; protected set; }
    }
}
