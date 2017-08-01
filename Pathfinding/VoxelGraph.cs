using System.Collections.Generic;

namespace Pathfinding
{
    public class VoxelGraph
    {
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, Node>>> _nodes = new Dictionary<int, Dictionary<int, Dictionary<int, Node>>>();

        public void AddNode(int xPos, int yPos, int zPos)
        {
            _nodes[xPos][yPos][zPos] = new Node(new Vector3(xPos, yPos, zPos));
        }

        public void ConnectNeighbours(int xPos, int yPos, int zPos)
        {
            var node = _nodes[xPos][yPos][zPos];
            if (node == null)
                return;
            for (var dX = -1; dX >= 1; dX++)
            {
                for (var dY = -1; dY >= 1; dY++)
                {
                    for (var dZ = -1; dZ >= 1; dZ++)
                    {
                        if (dX == 0 && dY == 0 && dZ == 0)
                            continue;
                        if (_nodes[xPos + dX][yPos + dY][zPos + dZ] != null)
                            _nodes[xPos + dX][yPos + dY][zPos + dZ].ConnectTo(node, Mathf.Sqrt(dX*dX+dY*dY+dZ*dZ));

                    }
                }
            }
        }

        public Node GetNode(Vector3 from)
        {
            return _nodes[(int) from.x][(int) from.y][(int) from.z];
        }
    }
    

    public class Node
    {
        public Dictionary<Node, float> Neighbours = new Dictionary<Node, float>();
        public Vector3 Position;

        public Node(Vector3 position)
        {
            Position = position;
        }

        public void ConnectTo(Node node, float dist)
        {
            Neighbours.Add(node, dist);
            node.Neighbours.Add(this, dist);
        }
    }
}
