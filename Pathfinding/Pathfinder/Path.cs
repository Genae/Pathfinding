using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding.Graphs;
using Pathfinding.Utils;

namespace Pathfinding.Pathfinder
{
    public class Path : Promise
    {
        public List<Node> Nodes;
        public float Length;
        public bool IsT0;

        protected Path(List<Node> nodes, float length, bool t0)
        {
            Nodes = nodes;
            Length = length;
            IsT0 = t0;
        }

        public Node GetNode(int i)
        {
            if (i > Nodes.Count - 1)
            {
                return null;
            }
            return Nodes[i];
        }
        
        public static Path Calculate(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            if ((from - to).magnitude < 200)
            {
                return CalculateLowlevelPath(graph, from, to);
            }
            return CalculateHighlevelPath(graph, @from, to);
        }

        private static Path CalculateHighlevelPath(VoxelGraph graph, Vector3I @from, Vector3I to)
        {
            var path = new Path(null, 0, false);
            path.Task = new Task(() =>
            {
                path = AStar.GetPath(graph.GetNode(from).SuperNodes.ToDictionary(n => n.Key as Node, n => n.Value.Length), graph.GetNode(to).GetClosestSuperNode(), path);
                path.Finished = true;
            });
            path.Task.Start();
            return path;
        }

        private static Path CalculateLowlevelPath(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            var path = new Path(null, 0, true);
            path.Task = new Task(() =>
            {
                path = AStar.GetPath(graph.GetNode(from), graph.GetNode(to), path);
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
}