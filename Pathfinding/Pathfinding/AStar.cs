using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pathfinding.Util;

namespace Pathfinding
{
    public class AStar
    {
        public static Path GetPath(VoxelGraph graph, Vector3I from, Vector3I to, Path path)
        {
            var start = DateTime.Now;
            var openSet = new PriorityQueue<PathNode>();
            var pathNodeMap = new Dictionary<Node, PathNode>();
            var size = graph.GetSize();
            var statusSet = new NodeStatus[size[0], size[1], size[2]];
            var nodeFrom = new PathNode(graph.GetNode(from), null, 0);
            var nodeTo = new PathNode(graph.GetNode(to), null, 0);
            if (nodeTo.GridNode == null || nodeFrom.GridNode == null)
            {
                //Debug.Log("Could not find start or end Node.");
                return null;
            }
            openSet.Enqueue(nodeFrom, (int)(nodeFrom.GetCost(nodeTo) * 10));

            while (!openSet.IsEmpty())
            {
                var curNode = openSet.Dequeue();
                if (curNode.Equals(nodeTo))
                {
                    path = ReconstructPath(curNode, path);
                    //Debug.Log("Found path between " + nodeFrom.GridNode.Position + " and " + nodeTo.GridNode.Position + " of length: " + path.Length + " in " + (DateTime.Now-start).TotalMilliseconds + "ms.");
                    return path;
                }
                //closedHashSet.Add(curNode.GridNode.Position);
                SetStatus(curNode.GridNode.Position, NodeStatus.Closed, statusSet);
                foreach (var neighbour in curNode.GridNode.GetNeighbours())
                {
                    var status = GetStatus(neighbour.To.Position, statusSet);
                    if (status == NodeStatus.Closed)
                        continue;
                    if (status == NodeStatus.Opened)
                    {
                        var node = new PathNode(neighbour.To, curNode, neighbour.Length);
                        if (openSet.Update(pathNodeMap[neighbour.To], (int) (pathNodeMap[neighbour.To].GetCost(nodeTo) * 10), node, (int) (node.GetCost(nodeTo) * 10)))
                            pathNodeMap[neighbour.To] = node;
                    }
                    else
                    {
                        var node = new PathNode(neighbour.To, curNode, neighbour.Length);
                        openSet.Enqueue(node, (int)(node.GetCost(nodeTo) * 10));
                        pathNodeMap[neighbour.To] = node;
                        SetStatus(neighbour.To.Position, NodeStatus.Opened, statusSet);
                    }
                }
            }
            //Debug.Log("Couldn't find path between " + nodeFrom.GridNode + " and " + nodeTo.GridNode + " in " + (DateTime.Now - start).TotalMilliseconds + "ms.");
            return path;
        }

        private static void SetStatus(Vector3I pos, NodeStatus status, NodeStatus[,,] statusSet)
        {
            statusSet[pos.x, pos.y, pos.z] = status;
        }

        private static NodeStatus GetStatus(Vector3I pos, NodeStatus[,,] statusSet)
        {
            return statusSet[pos.x, pos.y, pos.z];
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

        protected Path(List<Node> nodes, float length)
        {
            Nodes = nodes;
            Length = length;
        }

        public Node GetNode(int i)
        {
            if (i > Nodes.Count - 1)
            {
                return null;
            }
            return Nodes[i];
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

        public static Path Calculate(VoxelGraph graph, Vector3I from, Vector3I to)
        {
            var path = new Path(null, 0);
            //path.Task = new Task(() => UNITY3D
            path.Task = new Task(()=>
            {
                path = AStar.GetPath(graph, from, to, path);
                path.Finished = true;
            });
            path.Task.Start();
            return path;
        }
    }

    public class PathNode
    {
        public Node GridNode;
        public PathNode Prev;
        public float GScore;

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
