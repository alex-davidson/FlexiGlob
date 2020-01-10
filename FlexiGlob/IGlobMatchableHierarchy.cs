using System.Collections.Generic;

namespace FlexiGlob
{
    public interface IGlobMatchableHierarchy<T>
    {
        bool CaseSensitive { get; }
        T Root { get; }
        string GetName(T item);
        bool IsContainer(T item);
        IEnumerable<T> GetChildrenMatchingPrefix(T item, string prefix);
    }
}
