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
        
        public void Enqueue(T item, float priority)
        {
            var prio = PrioToInt(priority);
            if (!_storage.ContainsKey(prio))
            {
                if (_lowestPrio < 0 || _lowestPrio > prio)
                    _lowestPrio = prio;
                _storage.Add(prio, new List<T>());
            }
            _storage[prio].Add(item);
            _totalSize++;

        }

        private static int PrioToInt(float priority)
        {
            return (int) (priority * 10);
        }

        public float GetPrio()
        {
            return _lowestPrio;
        }
        
        public bool Update(T oldObj, float oldPrio, T newObj, float newPrio)
        {
            if (oldPrio <= newPrio)
                return false;
            Dequeue(oldObj, oldPrio);
            Enqueue(newObj, newPrio);
            return true;
        }

        private void Dequeue(T oldObj, float priority)
        {
            var prio = PrioToInt(priority);
            var q = _storage[prio];
            var index = q.IndexOf(oldObj);
            q[index] = q[q.Count - 1];
            q.RemoveAt(q.Count - 1);
            _totalSize--;
            if (q.Count != 0)
                return;
            _storage.Remove(prio);
            if (IsEmpty())
                _lowestPrio = -1;
            else if(prio == _lowestPrio)
                _lowestPrio = _storage.First().Key;
        }
    }
}
