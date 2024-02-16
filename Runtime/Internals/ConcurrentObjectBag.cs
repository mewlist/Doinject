using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Doinject
{
    internal sealed class ConcurrentObjectBag : IEnumerable<object>
    {
        private readonly ConcurrentBag<object> bag = new();

        public void Add(object instance)
        {
            bag.Add(instance);
        }

        public bool Any()
        {
            return bag.Any();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return bag.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            bag.Clear();
        }

        public int Count => bag.Count;
    }
}