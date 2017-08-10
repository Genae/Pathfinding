using System.Collections.Generic;
using System.Linq;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public abstract class Node
    {
        public Dictionary<SuperNode, SuperNodeConnection> SuperNodes = new Dictionary<SuperNode, SuperNodeConnection>();
        public Vector3I Position;

        public bool HasDirectSupernode => SuperNodes.Any(kv => kv.Value.Length <= kv.Value.SuperNode.GridSize);

        protected Node(Vector3I position)
        {
            Position = position;
        }

        public void ConnectSuperNode(Node from, SuperNode superNode, float dist)
        {
            var directChild = dist <= superNode.GridSize;
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
                    if (snode == superNode || SuperNodes[snode].Length > superNode.GridSize)
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

        public virtual void Delete(VoxelGraph graph)
        {
            foreach (var neighbour in GetNeighbours())
            {
                neighbour.To.RemoveNeighbour(this);
            }
            var directSuperNode = SuperNodes.FirstOrDefault(sn => sn.Value.Length.Equals(0)).Value;
            if (directSuperNode != null)
            {
                directSuperNode.SuperNode.Delete(graph);
            }
            foreach (var superNode in SuperNodes.Keys)
            {
                superNode.RemoveChildNode(this);
                foreach (var neighbour in GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode) && Equals(n.To.SuperNodes[superNode].To)))
                {
                    neighbour.To.RecalculateSuperNodePathAfterDelete(superNode, graph);
                }
            }
        }
        
        public void ConnectToSupernodes()
        {
            foreach (var neighbour in GetNeighbours())
            {
                foreach (var superNodeConnection in neighbour.To.SuperNodes.ToList())
                {
                    if (!SuperNodes.ContainsKey(superNodeConnection.Key))
                        RecalculateSuperNodePathAfterAdd(superNodeConnection.Key);
                }
            }
        }

        public bool RecalculateSuperNodePathAfterDelete(SuperNode superNode, VoxelGraph graph)
        {
            SuperNodeConnection old = null;
            if (SuperNodes.ContainsKey(superNode))
            {
                old = SuperNodes[superNode];
                SuperNodes.Remove(superNode);
            }
            var neighbours = GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode)).ToList();
            var queue = new PriorityQueue<Edge>();
            foreach (var neighbour in neighbours)
            {
                queue.Enqueue(neighbour, neighbour.To.SuperNodes[superNode].Length + neighbour.Length);
            }
            while (!queue.IsEmpty())
            {
                var n = queue.Dequeue();
                if (!n.To.SuperNodes.ContainsKey(superNode))
                    continue;
                if (Equals(n.To.SuperNodes[superNode].To))
                {
                    if (n.To.RecalculateSuperNodePathAfterDelete(superNode, graph))
                    {
                        queue.Enqueue(n, n.To.SuperNodes[superNode].Length + n.Length);
                    }
                }
                else
                {
                    var dist = n.Length + n.To.SuperNodes[superNode].Length;
                    if (old != null && old.To.Equals(n.To) && old.Length.Equals(dist))
                    {
                        ConnectSuperNode(n.To, superNode, dist);
                    }
                    else
                    {
                        ConnectSuperNode(n.To, superNode, dist);
                        graph.MarkDirty(this);
                        foreach (var neighbour in GetNeighbours().Where(ne => ne.To.SuperNodes.ContainsKey(superNode) && Equals(ne.To.SuperNodes[superNode].To)))
                        {
                            neighbour.To.RecalculateSuperNodePathAfterDeleteRec(superNode, graph);
                        }
                    }
                    return true;
                }
            }
            graph.MarkDirty(this);
            superNode.RemoveChildNode(this);
            return false;
        }

        private void RecalculateSuperNodePathAfterDeleteRec(SuperNode superNode, VoxelGraph graph)
        {
            var neighbours = GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode));
            float length = 0;
            Edge closest = null;
            foreach (var neighbour in neighbours)
            {
                var curDist = neighbour.Length + neighbour.To.SuperNodes[superNode].Length;
                if (closest == null)
                {
                    closest = neighbour;
                    length = curDist;
                }
                else if(length > curDist)
                {
                    closest = neighbour;
                    length = curDist;
                }
            }
            if (closest == null || closest.To.Equals(SuperNodes[superNode].To))
            {
                if (SuperNodes[superNode].Length.Equals(length))
                    return;
                SuperNodes[superNode].Length = length;
            }
            else
            {
                SuperNodes.Remove(superNode);
                ConnectSuperNode(closest.To, superNode, length);
                foreach (var neighbour in GetNeighbours().Where(ne => ne.To.SuperNodes.ContainsKey(superNode) && Equals(ne.To.SuperNodes[superNode].To)))
                {
                    neighbour.To.RecalculateSuperNodePathAfterDeleteRec(superNode, graph);
                }
            }
            if(length >= superNode.GridSize)
                graph.MarkDirty(this);
        }

        public void RecalculateSuperNodePathAfterAdd(SuperNode superNode)
        {
            var neighbours = GetNeighbours().Where(n => n.To.SuperNodes.ContainsKey(superNode)).ToList();
            var queue = new PriorityQueue<Edge>();
            foreach (var neighbour in neighbours)
            {
                queue.Enqueue(neighbour, neighbour.To.SuperNodes[superNode].Length + neighbour.Length);
            }
            while (!queue.IsEmpty())
            {
                var n = queue.Dequeue();
                if (!Equals(n.To.SuperNodes[superNode].To))
                {
                    var dist = n.Length + n.To.SuperNodes[superNode].Length;
                    if (!SuperNodes.ContainsKey(superNode) || SuperNodes[superNode].Length > dist)
                    {
                        ConnectSuperNode(n.To, superNode, dist);
                        foreach (var neighbour in neighbours.Where(ne => Equals(ne.To)))
                        {
                            neighbour.To.RecalculateSuperNodePathAfterAdd(superNode);
                        }
                        return;
                    }
                }
            }
        }

        public void KillSuperNode(SuperNode node, VoxelGraph graph)
        {
            if(SuperNodes[node].Length <= node.GridSize)
                graph.MarkDirty(this);
            SuperNodes.Remove(node);
            foreach (var neighbour in GetNeighbours())
            {
                if (neighbour.To.SuperNodes.ContainsKey(node))
                    neighbour.To.KillSuperNode(node, graph);
            }
        }
    }
}