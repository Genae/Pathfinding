using System;
using System.Collections.Generic;
using System.Text;

namespace Pathfinding
{
    public class Graph
    {
        private Dictionary<int, Dictionary<int, Dictionary<int, Node>>> _nodes = new Dictionary<int, Dictionary<int, Dictionary<int, Node>>>();

        public void AddNode(Node node, Vector3 position)
        {
            _nodes[(int)position.x][(int)position.y][(int)position.z] = node;
        }
    }
    

    public class Node
    {
    }
}
