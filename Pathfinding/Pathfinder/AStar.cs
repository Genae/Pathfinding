using System.Collections.Generic;
using Pathfinding.Graphs;
using Pathfinding.Utils;

namespace Pathfinding.Pathfinder
{
    public class AStar
    {
        public static Path GetPath(Node from, Node to, Path path)
        {
            return GetPath(new Dictionary<Node, float> {{from, 0f}}, to, path);
        }

        public static Path GetPath(Dictionary<Node, float> from, Node to, Path path)
        {
            var openSet = new PriorityQueue<VisitedNode>();
            var pathNodeMap = new Dictionary<Node, VisitedNode>();
            var nodeTo = new VisitedNode(to, null, 0);
            foreach (var f in from)
            {
                var nodeFrom = new VisitedNode(f.Key, null, f.Value);
                openSet.Enqueue(nodeFrom, (int)(nodeFrom.GetCost(nodeTo) * 10));
            }
            if (nodeTo.GridNode == null)
            {
                //Debug.Log("Could not find start or end Node.");
                return null;
            }
            while (!openSet.IsEmpty())
            {
                var curNode = openSet.Dequeue();
                if (curNode.Equals(nodeTo))
                {
                    path = ReconstructPath(curNode, path);
                    //Debug.Log("Found path between " + nodeFrom.GridNode.Position + " and " + nodeTo.GridNode.Position + " of length: " + path.Length + " in " + (DateTime.Now-start).TotalMilliseconds + "ms.");
                    return path;
                }
                curNode.Status = NodeStatus.Closed;
                foreach (var neighbour in curNode.GridNode.GetNeighbours())
                {
                    if (pathNodeMap.ContainsKey(neighbour.To))
                    {
                        var pathNode = pathNodeMap[neighbour.To];
                        if (pathNode.Status == NodeStatus.Closed)
                            continue;
                        var node = new VisitedNode(neighbour.To, curNode, neighbour.Length);
                        if (openSet.Update(pathNode, (int) (pathNode.GetCost(nodeTo) * 10), node, (int) (node.GetCost(nodeTo) * 10)))
                            pathNodeMap[neighbour.To] = node;
                    }
                    else
                    {
                        var node = new VisitedNode(neighbour.To, curNode, neighbour.Length);
                        openSet.Enqueue(node, (int)(node.GetCost(nodeTo) * 10));
                        pathNodeMap[neighbour.To] = node;
                    }
                }
            }
            //Debug.Log("Couldn't find path between " + nodeFrom.GridNode + " and " + nodeTo.GridNode + " in " + (DateTime.Now - start).TotalMilliseconds + "ms.");
            return path;
        }
        
        private static Path ReconstructPath(VisitedNode node, Path path)
        {
            var length = node.GScore;
            var nodes = new List<Node>();
            while (node != null)
            {
                nodes.Add(node.GridNode);
                node = node.Prev;
            }
            nodes.Reverse();
            path.Nodes = nodes;
            path.Length = length;
            return path;
        }
        
    }

    public enum NodeStatus
    {
        None,
        Opened,
        Closed
    }
}
