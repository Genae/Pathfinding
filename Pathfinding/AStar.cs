using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding.Util;

namespace Pathfinding
{
    public class AStar
    {
        public static Path GetPath(Node from, Node to, Path path)
        {
            return GetPath(new Dictionary<Node, float> {{from, 0f}}, to, path);
        }

        public static Path GetPath(Dictionary<Node, float> from, Node to, Path path)
        {
            var openSet = new PriorityQueue<PathNode>();
            var pathNodeMap = new Dictionary<Node, PathNode>();
            var nodeTo = new PathNode(to, null, 0);
            foreach (var f in from)
            {
                var nodeFrom = new PathNode(f.Key, null, f.Value);
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
                    var pathNode = pathNodeMap[neighbour.To];
                    if (pathNode != null)
                    {
                        if (pathNode.Status == NodeStatus.Closed)
                            continue;
                        var node = new PathNode(neighbour.To, curNode, neighbour.Length);
                        if (openSet.Update(pathNode, (int) (pathNode.GetCost(nodeTo) * 10), node, (int) (node.GetCost(nodeTo) * 10)))
                            pathNodeMap[neighbour.To] = node;
                    }
                    else
                    {
                        var node = new PathNode(neighbour.To, curNode, neighbour.Length);
                        openSet.Enqueue(node, (int)(node.GetCost(nodeTo) * 10));
                        pathNodeMap[neighbour.To] = node;
                    }
                }
            }
            //Debug.Log("Couldn't find path between " + nodeFrom.GridNode + " and " + nodeTo.GridNode + " in " + (DateTime.Now - start).TotalMilliseconds + "ms.");
            return path;
        }
        
        private static Path ReconstructPath(PathNode node, Path path)
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

    public class PathNode
    {
        public Node GridNode;
        public PathNode Prev;
        public float GScore;
        public NodeStatus Status = NodeStatus.Opened;

        public PathNode(Node node, PathNode prev, float cost)
        {
            GridNode = node;
            Prev = prev;
            if (prev == null)
            {
                GScore = 0;
            }
            else
            {
                GScore = prev.GScore + cost;
            }
        }

        public float GetCost(PathNode nodeTo)
        {
            var a = nodeTo.GridNode.Position;
            var b = GridNode.Position;
            var x = a.x - b.x;
            var y = a.y - b.y;
            var z = a.z - b.z;
            return GScore + (float)Math.Sqrt(x*x+y*y+z*z);
        }

        public bool Equals(PathNode node)
        {
            return node.GridNode.Equals(GridNode);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;
            var pn = (PathNode)obj;
            return pn.GridNode.Equals(GridNode);
        }

        public override int GetHashCode()
        {
            return (GridNode != null ? GridNode.GetHashCode() : 0);
        }
    }
}
