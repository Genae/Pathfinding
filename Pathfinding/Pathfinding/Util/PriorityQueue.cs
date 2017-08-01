using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.Util
{
    public class PriorityQueue<T>
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
            var minKey = _storage.First().Key;
            var q = _storage[minKey];
            _totalSize--;
            var deq = q.Dequeue();
            if (!q.Any())
            {
                _storage.Remove(minKey);
            }
            return deq;
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
    }
}
