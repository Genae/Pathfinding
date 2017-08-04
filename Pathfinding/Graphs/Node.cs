using System.Collections.Generic;
using System.Linq;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public abstract class Node
    {
        public Dictionary<SuperNode, SuperNodeConnection> SuperNodes = new Dictionary<SuperNode, SuperNodeConnection>();
        public Vector3I Position;

        protected Node(Vector3I position)
        {
            Position = position;
        }

        public void ConnectSuperNode(Node from, SuperNode superNode, float dist)
        {
            if (SuperNodes.ContainsKey(superNode))
            {
                if (SuperNodes[superNode].Length <= dist)
                    return;
            }
            else
            {
                superNode.ChildNodes.Add(this);
            }
            SuperNodes[superNode] = new SuperNodeConnection(from, superNode, dist);
            foreach (var snode in SuperNodes.Keys)
            {
                if (snode == superNode)
                    continue;
                snode.ConnectTo(superNode, dist, from);
            }
        }

        public Node GetClosestSuperNode()
        {
            if (SuperNodes.Count == 0)
                return null;
            var min = SuperNodes.Min(kv => kv.Value.Length);
            return SuperNodes.FirstOrDefault(n => Equals(n.Value.Length, min)).Key;
        }
        
        public abstract List<Edge> GetNeighbours();
    }
}