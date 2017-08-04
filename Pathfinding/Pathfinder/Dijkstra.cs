using System.Collections.Generic;
using Pathfinding.Graphs;
using Pathfinding.Pathfinder;
using Pathfinding.Utils;

namespace Pathfinding
{
    public class Dijkstra
    {
        public static void Fill(VoxelGraph graph, Vector3I position, int range, SuperNode supernode)
        {
            var openQueue = new PriorityQueue<VisitedNode>();
            var pathNodeMap = new Dictionary<Node, VisitedNode>();
            var startNode = graph.GetNode(position);
            pathNodeMap[startNode] = new VisitedNode(graph.GetNode(position), null, 0);
            openQueue.Enqueue(pathNodeMap[startNode], 0);
            while (!openQueue.IsEmpty())
            {
                var current = openQueue.Dequeue();
                current.GridNode.ConnectSuperNode(current.Prev?.GridNode ?? supernode, supernode, current.GScore);
                foreach (var neighbour in current.GridNode.GetNeighbours())
                {
                    if (!neighbour.To.SuperNodes.ContainsKey(supernode) && neighbour.Length + current.GScore < range)
                    {
                        var newNode = new VisitedNode(neighbour.To, current, neighbour.Length);
                        if (pathNodeMap.ContainsKey(neighbour.To))
                        {
                            if(openQueue.Update(pathNodeMap[neighbour.To], (int)(pathNodeMap[neighbour.To].GScore * 10), newNode, (int)(newNode.GScore * 10)))
                                pathNodeMap[neighbour.To] = newNode;
                        }
                        else
                        {
                            openQueue.Enqueue(newNode, (int)(newNode.GScore*10));
                            pathNodeMap[neighbour.To] = newNode;
                        }
                    }
                }
            }
        }
    }
}
