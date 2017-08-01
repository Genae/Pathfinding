using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pathfinding
{
    public class VoxelGraph
    {
        private readonly Grid3D<Node> _grid = new Grid3D<Node>();

        public void AddNode(int xPos, int yPos, int zPos)
        {
            _grid[xPos, yPos, zPos] = new Node(new Vector3(xPos, yPos, zPos));
            ConnectNeighbours(xPos, yPos, zPos);
        }

        public void ConnectNeighbours(int xPos, int yPos, int zPos)
        {
            var node = _grid[xPos, yPos, zPos];
            if (node == null)
                return;
            for (var dX = -1; dX <= 1; dX++)
            {
                for (var dY = -1; dY <= 1; dY++)
                {
                    for (var dZ = -1; dZ <= 1; dZ++)
                    {
                        if (dX == 0 && dY == 0 && dZ == 0)
                            continue;
                        if (_grid[xPos + dX, yPos + dY, zPos + dZ] != null)
                            _grid[xPos + dX, yPos + dY, zPos + dZ].ConnectTo(node, Mathf.Sqrt(dX*dX+dY*dY+dZ*dZ));

                    }
                }
            }
        }

        public Vector3 GetSize()
        {
            return _grid.GetSize();
        }

        public Node GetNode(Vector3 from)
        {
            return _grid[(int) from.x, (int) from.y, (int) from.z];
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

    public class Grid3D<T>
    {
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, T>>> _nodes = new Dictionary<int, Dictionary<int, Dictionary<int, T>>>();
        private Vector3 _size;

        public T this[int xPos, int yPos, int zPos]
        {
            get => _nodes.ContainsKey(xPos) && _nodes[xPos].ContainsKey(yPos) && _nodes[xPos][yPos].ContainsKey(zPos) ? _nodes[xPos][yPos][zPos] : default(T);
            set
            {
                _size = default(Vector3);
                if (!_nodes.ContainsKey(xPos))
                    _nodes[xPos] = new Dictionary<int, Dictionary<int, T>>();
                if (!_nodes[xPos].ContainsKey(yPos))
                    _nodes[xPos][yPos] = new Dictionary<int, T>();
                _nodes[xPos][yPos][zPos] = value; }
        }

        public Vector3 GetSize()
        {
            if (_size == default(Vector3))
            {
                _size = new Vector3();
                _size.x = _nodes.Max(v => v.Key) + 1;
                _size.y = _nodes.Values.Max(yDics => yDics.Max(v => v.Key)) + 1;
                _size.z = _nodes.Values.Max(yDics => yDics.Values.Max(zDics => zDics.Max(v => v.Key))) + 1;
            }
            return _size;
        }
    }
}
