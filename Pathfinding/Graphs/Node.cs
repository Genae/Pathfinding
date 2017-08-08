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

        public void ConnectSuperNode(Node from, SuperNode superNode, float dist, bool directChild = true)
        {
            if (SuperNodes.ContainsKey(superNode))
            {
                if (SuperNodes[superNode].Length <= dist)
                    return;
            }
            else
            {
                if(directChild)
                    superNode.ChildNodes.Add(this);
            }
            SuperNodes[superNode] = new SuperNodeConnection(from, superNode, dist);
            if (directChild)
            {
                foreach (var snode in SuperNodes.Keys)
                {
                    if (snode == superNode)
                        continue;
                    snode.ConnectTo(superNode, dist, from);
                }
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
        protected abstract void RemoveNeighbour(Node node);

        public void Delete()
        {
            foreach (var neighbour in GetNeighbours())
            {
                neighbour.To.RemoveNeighbour(this);
            }
            foreach (var superNode in SuperNodes.Keys)
            {
                superNode.RemoveChildNode(this);
                foreach (var neighbour in GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode) && n.To.SuperNodes[superNode].To.Equals(this)))
                {
                    neighbour.To.RecalculateSuperNodePathTo(superNode);
                }
            }
        }

        public bool RecalculateSuperNodePathTo(SuperNode superNode)
        {
            SuperNodes.Remove(superNode);
            var neighbours = GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode)).ToList();
            var queue = new PriorityQueue<Edge>();
            foreach (var neighbour in neighbours)
            {
                queue.Enqueue(neighbour, neighbour.To.SuperNodes[superNode].Length + neighbour.Length);
            }
            while (!queue.IsEmpty())
            {
                var n = queue.Dequeue();
                if (n.To.SuperNodes[superNode].To.Equals(this))
                {
                    if (n.To.RecalculateSuperNodePathTo(superNode))
                    {
                        queue.Enqueue(n, n.To.SuperNodes[superNode].Length + n.Length);
                    }
                }
                else
                {
                    var dist = n.Length + n.To.SuperNodes[superNode].Length;
                    ConnectSuperNode(n.To, superNode, dist);
                    return true;
                }
            }
            superNode.RemoveChildNode(this);
            return false;
        }
    }
}