using System.Collections.Generic;
using Pathfinding.Graphs;

namespace Pathfinding.Pathfinder
{
    public class PathRegistry
    {
        public HashSet<Path> ActivePaths = new HashSet<Path>();

        public void RemovedNode(Node node, VoxelGraph graph)
        {
            foreach (var activePath in ActivePaths)
            {
                if (activePath.Nodes.Contains(node))
                    activePath.Recalculate(node, graph);
            }
        }
    }
}
