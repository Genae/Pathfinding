﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding.Util;

namespace Pathfinding
{
    public class VoxelGraph
    {
        private readonly Grid3D<Node> _grid = new Grid3D<Node>();
        private readonly Grid3D<Node> _gridT1 = new Grid3D<Node>();

        public void AddNode(int xPos, int yPos, int zPos)
        {
            _grid[xPos, yPos, zPos] = new Node(new Vector3I(xPos, yPos, zPos));
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

        public void AddTier1Nodes(int gridSize)
        {
            var size = GetSize();
            for (var dX = Math.Min(gridSize/2, size[0]); dX <= size[0]; dX+=gridSize)
            {
                for (var dY = Math.Min(gridSize / 2, size[1]) / 2; dY <= size[1]; dY += gridSize)
                {
                    for (var dZ = Math.Min(gridSize / 2, size[2]); dZ <= size[2]; dZ += gridSize)
                    {
                        var node = _grid.GetNearestItem(new Vector3I(dX, dY, dZ), 5);
                        if (node == null)
                            continue;
                        _gridT1[dX, dY, dZ] = new Node(node.Position);
                    }
                }
            }
        }

        public Vector3I GetSize()
        {
            return _grid.GetSize();
        }

        public Node GetNode(Vector3I from)
        {
            return _grid[from.x, from.y, from.z];
        }

        public IEnumerable<Node> GetT1Nodes()
        {
            return _gridT1;
        }
    }
    

    public class Node
    {
        public List<Edge> Neighbours = new List<Edge>();
        public Vector3I Position;

        public Node(Vector3I position)
        {
            Position = position;
        }

        public void ConnectTo(Node node, float dist)
        {
            Neighbours.Add(new Edge(this, node, dist));
            node.Neighbours.Add(new Edge(node, this, dist));
        }
    }

    public class SuperNode : Node
    {

        public SuperNode(Vector3I position) : base(position)
        {
        }


    }

    public class Edge
    {
        public Node To;
        public Node From;
        public float Length;

        public Edge(Node from, Node to, float length)
        {
            From = from;
            To = to;
            Length = length;
        }
    }

    public class SuperNodeConnection : Edge
    {
        public SuperNode SuperNode;

        public SuperNodeConnection(Node from, Node to, SuperNode superNode, float length) : base(from, to, length)
        {
            SuperNode = superNode;
        }
    }

    public class Grid3D<T> : IEnumerable<T> where T: class
    {
        private readonly Dictionary<int, Dictionary<int, Dictionary<int, T>>> _nodes = new Dictionary<int, Dictionary<int, Dictionary<int, T>>>();
        private Vector3I _size;

        public T this[int xPos, int yPos, int zPos]
        {
            get => _nodes.ContainsKey(xPos) && _nodes[xPos].ContainsKey(yPos) && _nodes[xPos][yPos].ContainsKey(zPos) ? _nodes[xPos][yPos][zPos] : null;
            set
            {
                _size = default(Vector3I);
                if (!_nodes.ContainsKey(xPos))
                    _nodes[xPos] = new Dictionary<int, Dictionary<int, T>>();
                if (!_nodes[xPos].ContainsKey(yPos))
                    _nodes[xPos][yPos] = new Dictionary<int, T>();
                _nodes[xPos][yPos][zPos] = value; }
        }

        public T this[Vector3I v]
        {
            get => this[v.x, v.y, v.z];
            set => this[v.x, v.y, v.z] = value;
        }

        public T GetNearestItem(Vector3I pos, int maxRadius)
        {
            for (var dX = 0; dX <= maxRadius; dX++)
            {
                for (var dY = 0; dY <= maxRadius; dY++)
                {
                    for (var dZ = 0; dZ <= maxRadius; dZ++)
                    {
                        var node = this[pos.x + dX, pos.y + dY, pos.z + dZ] ?? this[pos.x - dX, pos.y + dY, pos.z + dZ] ??
                               this[pos.x + dX, pos.y - dY, pos.z + dZ] ?? this[pos.x - dX, pos.y - dY, pos.z + dZ] ??
                               this[pos.x + dX, pos.y + dY, pos.z - dZ] ?? this[pos.x - dX, pos.y + dY, pos.z - dZ] ??
                               this[pos.x + dX, pos.y - dY, pos.z - dZ] ?? this[pos.x - dX, pos.y - dY, pos.z - dZ];
                        if (node != null)
                            return node;

                    }
                }
            }
            return null;
        }

        public Vector3I GetSize()
        {
            if (_size == default(Vector3I))
            {
                var x = _nodes.Max(v => v.Key) + 1;
                var y = _nodes.Values.Max(yDics => yDics.Max(v => v.Key)) + 1;
                var z = _nodes.Values.Max(yDics => yDics.Values.Max(zDics => zDics.Max(v => v.Key))) + 1;
                _size = new Vector3I(x, y, z);
            }
            return _size;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _nodes.Values.SelectMany(x => x.Values.SelectMany(y => y.Values.Select(z => z))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
