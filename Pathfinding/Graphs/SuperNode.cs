using System.Collections.Generic;
using System.Linq;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public class SuperNode : Node
    {
        public readonly int GridSize;
        public HashSet<Node> ChildNodes = new HashSet<Node>();
        private readonly Dictionary<SuperNode, Edge> _neighbours = new Dictionary<SuperNode, Edge>(); 

        public SuperNode(Vector3I position, int gridSize) : base(position)
        {
            GridSize = gridSize;
        }

        public bool ConnectTo(SuperNode node, float dist, Node via)
        {
            SuperNodeEdge old = null;
            if (_neighbours.ContainsKey(node))
                 old = (SuperNodeEdge)_neighbours[node];
            if (old == null || old.Length >= dist)
            {
                _neighbours[node] = new SuperNodeEdge(node, dist, via);
                node._neighbours[this] = new SuperNodeEdge(this, dist, via);
                return true;
            }
            return false;
        }
        
        public override List<Edge> GetNeighbours()
        {
            return _neighbours.Values.ToList();
        }

        public override void Delete(VoxelGraph graph)
        {
            var mainChild = graph.GetNode(Position);
            mainChild.KillSuperNode(this, graph);
            base.Delete(graph);
        }

        protected override void RemoveNeighbour(Node node)
        {
            var k = _neighbours.FirstOrDefault(e => e.Value.To.Equals(node)).Key;
            if (k != null)
                _neighbours.Remove(k);
        }

        public void RemoveChildNode(Node node)
        {
            ChildNodes.Remove(node);
        }
    }
}