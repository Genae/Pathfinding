using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.Util
{
    public class PriorityQueue<T> : IEnumerable<T>
    {
        private int _totalSize;
        private readonly SortedDictionary<int, Queue<T>> _storage;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<int, Queue<T>>();
            _totalSize = 0;
        }

        public bool IsEmpty()
        {
            return (_totalSize == 0);
        }

        public T Dequeue()
        {
            if (IsEmpty())
            {
                return default(T);
            }
            foreach (var q in _storage.Values.Where(q => q.Count > 0))
            {
                _totalSize--;
                return q.Dequeue();
            }
            return default(T);
        }

        // same as above, except for peek.

        public T Peek()
        {
            if (IsEmpty())
                return default(T);
            foreach (var q in _storage.Values.Where(q => q.Count > 0))
            {
                return q.Peek();
            }
            return default(T); // not supposed to reach here.
        }

        public T Dequeue(int prio)
        {
            _totalSize--;
            return _storage[prio].Dequeue();
        }

        public T Dequeue(T item)
        {
            _totalSize--;
            var queue = _storage.Values.First(q => q.Contains(item));
            T cur;
            while (!(cur = queue.Dequeue()).Equals(item))
            {
                queue.Enqueue(cur);
            }
            return item;
        }

        public void Enqueue(T item, int prio)
        {
            if (!_storage.ContainsKey(prio))
            {
                _storage.Add(prio, new Queue<T>());
            }
            _storage[prio].Enqueue(item);
            _totalSize++;

        }

        public PriorityQueue<T> Copy()
        {
            var pq = new PriorityQueue<T>();
            foreach (var key in _storage.Keys)
            {
                foreach (var item in _storage[key])
                {
                    pq.Enqueue(item, key);
                }
            }
            return pq;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.Values.SelectMany(v => v).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
