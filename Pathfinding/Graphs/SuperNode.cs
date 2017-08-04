using System.Collections.Generic;
using System.Linq;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public class SuperNode : Node
    {
        public List<Node> ChildNodes = new List<Node>();
        private readonly Dictionary<SuperNode, Edge> _neigbours = new Dictionary<SuperNode, Edge>(); 

        public SuperNode(Vector3I position) : base(position)
        {
        }

        public bool ConnectTo(SuperNode node, float dist, Node via)
        {
            var old = (SuperNodeEdge)_neigbours[node];
            if (old == null || old.Length >= dist)
            {
                _neigbours[node] = new SuperNodeEdge(node, dist, via);
                node._neigbours[this] = new SuperNodeEdge(this, dist, via);
                return true;
            }
            return false;
        }
        
        public override List<Edge> GetNeighbours()
        {
            return _neigbours.Values.ToList();
        }
    }
}