using System.Collections.Generic;
using System.Linq;
using Pathfinding.Graphs;
using Pathfinding.Pathfinder;

namespace Pathfinding.Agents
{
    public class MovingAgent
    {
        public List<Node> FollowPath(Path path, VoxelGraph graph)
        {
            if (path.IsT0)
            {
                return path.Nodes;
            }
            var lastSuper = (SuperNode)path.Nodes.Last();
            var pos = 0;
            var curNode = path.Start;
            var visited = new List<Node>();
            while (!curNode.SuperNodes.ContainsKey(lastSuper))
            {
                visited.Add(curNode);
                for (var i = 1; i >= 0; i--)
                {
                    if (curNode.SuperNodes.ContainsKey((SuperNode)path.Nodes[pos + i]))
                    {
                        curNode = curNode.SuperNodes[(SuperNode)path.Nodes[pos + i]].To;
                        pos += i;
                        break;
                    }                    
                }
                    
            }
            var finalPath = Path.Calculate(graph, curNode.Position, path.Target.Position);
            finalPath.Task.Wait();
            visited.AddRange(finalPath.Nodes);
            return visited;
        }
    }
}
