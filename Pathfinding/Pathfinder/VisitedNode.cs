using System;
using Pathfinding.Graphs;

namespace Pathfinding.Pathfinder
{
    public class VisitedNode
    {
        public Node GridNode;
        public VisitedNode Prev;
        public float GScore;
        public NodeStatus Status = NodeStatus.Opened;

        public VisitedNode(Node node, VisitedNode prev, float cost)
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

        public float GetCost(VisitedNode nodeTo)
        {
            var a = nodeTo.GridNode.Position;
            var b = GridNode.Position;
            var x = a.x - b.x;
            var y = a.y - b.y;
            var z = a.z - b.z;
            return GScore + (float)Math.Sqrt(x*x+y*y+z*z);
        }

        public bool Equals(VisitedNode node)
        {
            return node.GridNode.Equals(GridNode);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;
            var pn = (VisitedNode)obj;
            return pn.GridNode.Equals(GridNode);
        }

        public override int GetHashCode()
        {
            return (GridNode != null ? GridNode.GetHashCode() : 0);
        }
    }
}