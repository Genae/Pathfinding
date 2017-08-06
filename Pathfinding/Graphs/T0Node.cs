using System.Collections.Generic;
using System.Linq;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public class T0Node : Node
    {
        private readonly List<Edge> _neighbours = new List<Edge>();

        public T0Node(Vector3I position) : base(position)
        {
        }

        public void ConnectTo(Node node, float dist)
        {
            _neighbours.Add(new Edge(node, dist));
            node.GetNeighbours().Add(new Edge(this, dist));
        }

        public override List<Edge> GetNeighbours()
        {
            return _neighbours;
        }

        protected override void RemoveNeighbour(Node node)
        {
            var edge = _neighbours.FirstOrDefault(e => e.To.Equals(node));
            if(edge != null)
                _neighbours.Remove(edge);
        }
    }
}