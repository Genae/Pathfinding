using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.Util
{
    public class PriorityQueue<T>
    {
        private int _totalSize;
        private readonly SortedDictionary<int, Queue<T>> _storage;
        private readonly Dictionary<int, int> _count;
        private int _lowestPrio = -1;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<int, Queue<T>>();
            _count = new Dictionary<int, int>();
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
            var q = _storage[_lowestPrio];
            _totalSize--;
            _count[_lowestPrio]--;
            var deq = q.Dequeue();
            if (_count[_lowestPrio] == 0)
            {
                _storage.Remove(_lowestPrio);
                if (IsEmpty())
                    _lowestPrio = -1;
                else
                    _lowestPrio = _storage.First().Key;
            }
            return deq;
        }
        
        public void Enqueue(T item, int prio)
        {
            if (!_storage.ContainsKey(prio))
            {
                if (_lowestPrio < 0 || _lowestPrio > prio)
                    _lowestPrio = prio;
                _storage.Add(prio, new Queue<T>());
                _count[prio] = 0;
            }
            _storage[prio].Enqueue(item);
            _count[prio]++;
            _totalSize++;

        }

        public float GetPrio()
        {
            return _lowestPrio;
        }
        
        public bool Update(T oldObj, int oldPrio, T newObj, int newPrio)
        {
            if (oldPrio <= newPrio)
                return false;
            Dequeue(oldObj, oldPrio);
            Enqueue(newObj, newPrio);
            return true;
        }

        private void Dequeue(T oldObj, int oldPrio)
        {
            var queue = _storage[oldPrio];
            T cur;
            while (!(cur = queue.Dequeue()).Equals(oldObj))
            {
                queue.Enqueue(cur);
            }
            _count[oldPrio]--;
            _totalSize--;
            if (_count[oldPrio] == 0)
            {
                _storage.Remove(oldPrio);
                if (IsEmpty())
                    _lowestPrio = -1;
                else if(oldPrio == _lowestPrio)
                    _lowestPrio = _storage.First().Key;
            }
        }
    }
}
