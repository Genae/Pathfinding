using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.Utils
{
    public class PriorityQueue<T>
    {
        private int _totalSize;
        private readonly SortedDictionary<int, List<T>> _storage;
        private int _lowestPrio = -1;

        public PriorityQueue()
        {
            _storage = new SortedDictionary<int, List<T>>();
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
            var deq = q[q.Count-1];
            q.RemoveAt(q.Count-1);
            if (q.Count == 0)
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
                _storage.Add(prio, new List<T>());
            }
            _storage[prio].Add(item);
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
            var q = _storage[oldPrio];
            int index = q.IndexOf(oldObj);
            q[index] = q[q.Count - 1];
            q.RemoveAt(q.Count - 1);
            _totalSize--;
            if (q.Count == 0)
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
