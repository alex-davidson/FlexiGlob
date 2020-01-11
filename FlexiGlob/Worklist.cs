using System.Collections.Generic;

namespace FlexiGlob
{
    internal class Worklist<T>
    {
        private readonly Stack<T> inner = new Stack<T>();

        public void Add(T item)
        {
            inner.Push(item);
        }

        public bool TryTake(out T item)
        {
            item = default!;
            if (inner.Count == 0) return false;
            item = inner.Pop();
            return true;
        }
    }
}
