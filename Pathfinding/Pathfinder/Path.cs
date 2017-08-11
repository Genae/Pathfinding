using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding.Agents;
using Pathfinding.Graphs;
using Pathfinding.Utils;

namespace Pathfinding.Pathfinder
{
    public class Path : Promise, IDisposable
    {
        public List<Node> Nodes;
        public Node Start;
        public Node Target;
        public virtual float Length { get; set; }
        public bool IsT0;
        public PathState State;

        private int _currentNode;
        private readonly PathRegistry _registry;

        public Path(Node start, Node target, PathRegistry registry)
        {
            IsT0 = false;
            Start = start;
            Target = target;
            State = PathState.InitialCalulation;
            _registry = registry;
            _registry.ActivePaths.Add(this);
        }


        public Node GetNode(int i)
        {
            if(State.Equals(PathState.Invalid) || State.Equals(PathState.Recalculation) && i > _currentNode)
                throw new Exception("Path State is " + State);
            if (i > Nodes.Count - 1)
            {
                Dispose();
                return null;
            }
            _currentNode = i;
            return Nodes[i];
        }
        
        public static Path Calculate(VoxelGraph graph, Vector3I from, Vector3I to, bool forceOptimal = false)
        {
            if ((from - to).magnitude < 200 || forceOptimal)
            {
                return CalculateLowlevelPath(graph, from, to);
            }
            return CalculateHighlevelPath(graph, from, to);
        }

        private static Path CalculateHighlevelPath(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            var start = graph.GetNode(from);
            var target = graph.GetNode(to);
            var path = new HighLevelPath(start, target, graph);
            
            path.Task = new Task(() =>
            {
                path = (HighLevelPath)AStar.GetPath(start.SuperNodes.ToDictionary(n => n.Key as Node, n => n.Value.Length), target.GetClosestSuperNode(), path);
                path.Finished = true;
                path.State = PathState.Ready;
            });
            path.Task.Start();
            return path;
        }

        private static Path CalculateLowlevelPath(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            var start = graph.GetNode(from);
            var target = graph.GetNode(to);
            var path = new Path(start, target, graph.GetPathRegistry());
            path.Task = new Task(() =>
            {
                path = AStar.GetPath(start, target, path);
                path.Finished = true;
                path.State = PathState.Ready;
            });
            path.Task.Start();
            return path;
        }


        /*
         * UNITY3D Implementation
        public void Visualize(Color color, int fromNode = -1)
        {
            if (Nodes == null)
            {
                return;
            }
            for (int i = fromNode + 1; i < Nodes.Count - 1; i++)
            {
                if (fromNode == -1)
                    Debug.DrawLine(Nodes[i].Position, Nodes[i + 1].Position, color, 60000, true);
                else
                    Debug.DrawLine(Nodes[i].Position, Nodes[i + 1].Position, color);

            }
        }*/
        public void Recalculate(Node removedNode, VoxelGraph graph)
        {
            if (Target.Equals(removedNode))
            {
                State = PathState.Invalid;
            }
            else
            {
                State = PathState.Recalculation;
                var p2 = Calculate(graph, GetNode(_currentNode).Position, Target.Position);
                Task = p2.Task.ContinueWith(task =>
                {
                    Nodes = p2.Nodes;
                    Start = p2.Start;
                    Length = p2.Length;
                    IsT0 = p2.IsT0;
                    State = p2.State;
                    p2.Dispose();
                });

            }
        }

        public void Dispose()
        {
            _registry.ActivePaths.Remove(this);
        }
    }

    public enum PathState
    {
        InitialCalulation,
        Recalculation,
        Invalid,
        Ready
    }

    public class HighLevelPath : Path
    {
        private readonly VoxelGraph _graph;
        private Path _drilledDownPath;
        public Path ExactPath
        {
            get
            {
                if (Nodes == null || Nodes.Count == 0)
                    return null;
                return _drilledDownPath ?? (_drilledDownPath = CreateDrilledDownPath());
            }
        }

        private Path CreateDrilledDownPath()
        {
            var ag = new MovingAgent();
            return ag.FollowPath(this, _graph);
        }

        public override float Length
        {
            get { return ExactPath?.Length??0; }
            set { }
        }



        public HighLevelPath(Node start, Node target, VoxelGraph graph) : base(start, target, graph.GetPathRegistry())
        {
            _graph = graph;
        }
    }
}