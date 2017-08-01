using System;
using System.Threading.Tasks;

namespace Pathfinding.Util
{
    public abstract class Promise
    {
        public Task Task;
        private bool _finished;
        public bool Finished
        {
            get => _finished;
            set
            {
                _finished = value;
                if (_finished)
                {
                    OnFinish?.Invoke();
                }
            }
        }

        public Action OnFinish;
    }

    /*
     * UNITY 3D Implementations
    public abstract class Promise
    {
        protected Task Task;
        private bool _finished;
        public bool Finished
        {
            get { return _finished; }
            set
            {
                _finished = value;
                if (_finished && OnFinish != null)
                {
                    MainThread.Execute(OnFinish);
                }
            }
        }

        public Action OnFinish;
    }

    public class MainThread : MonoBehaviour
    {
        private static MainThread _mt;
        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        public static void Execute(Action a)
        {
            _mt._actionQueue.Enqueue(a);
        }

        void Update()
        {
            while (_actionQueue.Count > 0)
            {
                _actionQueue.Dequeue()();
            }
        }

        public static void Instantiate()
        {
            var mtObj = new GameObject { name = "MainThread" };
            _mt = mtObj.AddComponent<MainThread>();
        }
    }*/
}
