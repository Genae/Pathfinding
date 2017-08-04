using System;
using System.Collections.Generic;
using Pathfinding.Utils;

namespace Pathfinding.Graphs
{
    public class VoxelGraph
    {
        private readonly Grid3D<Node> _grid = new Grid3D<Node>();
        private readonly Grid3D<SuperNode> _gridT1 = new Grid3D<SuperNode>();

        public void AddNode(int xPos, int yPos, int zPos)
        {
            _grid[xPos, yPos, zPos] = new T0Node(new Vector3I(xPos, yPos, zPos));
            ConnectNeighbours(xPos, yPos, zPos);
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
                            (_grid[xPos + dX, yPos + dY, zPos + dZ] as T0Node).ConnectTo(node, Mathf.Sqrt(dX*dX+dY*dY+dZ*dZ));

                    }
                }
            }
        }

        public void AddTier1Nodes(int gridSize)
        {
            var size = GetSize();
            AddSupernodeGrid(gridSize, size);
            FillSupernodeHoles(gridSize, size);
        }
        
        private void AddSupernodeGrid(int gridSize, Vector3I size)
        { 
            for (var dX = Math.Min(gridSize / 2, size[0]); dX <= size[0]; dX += gridSize)
            {
                for (var dY = Math.Min(gridSize / 2, size[1]) / 2; dY <= size[1]; dY += gridSize)
                {
                    for (var dZ = Math.Min(gridSize / 2, size[2]); dZ <= size[2]; dZ += gridSize)
                    {
                        var node = _grid.GetNearestItem(new Vector3I(dX, dY, dZ), 5);
                        if (node == null)
                            continue;
                        var supernode = new SuperNode(node.Position);
                        _gridT1[dX, dY, dZ] = supernode;

                        Dijkstra.Fill(this, supernode.Position, gridSize, supernode);
                    }
                }
            }
        }

        private void FillSupernodeHoles(int gridSize, Vector3I size)
        {
            for (var dX = 0; dX <= size[0]; dX++)
            {
                for (var dY = 0; dY <= size[1]; dY++)
                {
                    for (var dZ = 0; dZ <= size[2]; dZ++)
                    {
                        var node = _grid[dX, dY, dZ];
                        if (node == null || node.SuperNodes.Count > 0)
                            continue;
                        var supernode = new SuperNode(node.Position);
                        _gridT1[dX, dY, dZ] = supernode;

                        Dijkstra.Fill(this, supernode.Position, gridSize, supernode);
                    }
                }
            }
        }
    }
}
