﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Pathfinding.Util;

namespace Pathfinding
{
    public class AStar
    {
        public static Path GetPath(VoxelGraph graph, Vector3 from, Vector3 to, Path path)
        {
            var start = DateTime.Now;
            var closeSet = new HashSet<Node>();
            var openSet = new PriorityQueue<PathNode>();
            var nodeFrom = new PathNode(graph.GetNode(from), null, 0);
            var nodeTo = new PathNode(graph.GetNode(to), null, 0);
            if (nodeTo.GridNode == null || nodeFrom.GridNode == null)
            {
                //Debug.Log("Could not find start or end Node.");
                return null;
            }
            openSet.Enqueue(nodeFrom, nodeFrom.GetCost(nodeTo));

            while (!openSet.IsEmpty())
            {
                var curNode = openSet.Dequeue();
                if (curNode.Equals(nodeTo))
                {
                    path = ReconstructPath(curNode, path);
                    //Debug.Log("Found path between " + nodeFrom.GridNode.Position + " and " + nodeTo.GridNode.Position + " of length: " + path.Length + " in " + (DateTime.Now-start).TotalMilliseconds + "ms.");
                    return path;
                }
                closeSet.Add(curNode.GridNode);
                foreach (var neighbour in curNode.GridNode.Neighbours)
                {
                    if (closeSet.Contains(neighbour.Key))
                        continue;
                    var node = new PathNode(neighbour.Key, curNode, neighbour.Value);
                    if (!openSet.Contains(node))
                    {
                        openSet.Enqueue(node, node.GetCost(nodeTo));
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

        public static Path Calculate(VoxelGraph graph, Vector3 from, Vector3 to)
        {
            var path = new Path(null, 0);
            //path.Thread = new Thread(() => UNITY3D
            path.Thread = Task.Run(()=>
            {
                path = AStar.GetPath(graph, from, to, path);
                path.Finished = true;
            });
            path.Thread.Start();
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

        public int GetCost(PathNode nodeTo)
        {
            return (int)(GScore + (nodeTo.GridNode.Position - GridNode.Position).magnitude);
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