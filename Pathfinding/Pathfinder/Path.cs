using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding.Agents;
using Pathfinding.Graphs;
using Pathfinding.Utils;

namespace Pathfinding.Pathfinder
{
    public class Path : Promise
    {
        public List<Node> Nodes;
        public Node Start;
        public Node Target;
        public virtual float Length { get; set; }
        public bool IsT0;

        public Path(Node start, Node target)
        {
            IsT0 = false;
            Start = start;
            Target = target;
        }


        public Node GetNode(int i)
        {
            if (i > Nodes.Count - 1)
            {
                return null;
            }
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
            });
            path.Task.Start();
            return path;
        }

        private static Path CalculateLowlevelPath(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            var start = graph.GetNode(from);
            var target = graph.GetNode(to);
            var path = new Path(start, target);
            path.Task = new Task(() =>
            {
                path = AStar.GetPath(start, target, path);
                path.Finished = true;
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



        public HighLevelPath(Node start, Node target, VoxelGraph graph) : base(start, target)
        {
            _graph = graph;
        }
    }
}