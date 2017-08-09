using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding.Graphs;
using Pathfinding.Pathfinder;

namespace Pathfinding.Agents
{
    public class MovingAgent
    {
        public Path FollowPath(Path path, VoxelGraph graph)
        {
            if (path.IsT0 || path.Nodes == null)
            {
                return path;
            }
            var lastSuper = (SuperNode)path.Nodes.Last();
            var pos = 0;
            var curNode = path.Start;
            var visited = new List<Node>();
            float length = 0;
            while (!curNode.SuperNodes.ContainsKey(lastSuper))
            {
                if(visited.Contains(curNode))
                    throw new Exception("Loop detected");
                visited.Add(curNode);
                for (var i = 1; i >= 0; i--)
                {
                    if (curNode.SuperNodes.ContainsKey((SuperNode)path.Nodes[pos + i]))
                    {
                        var nextStep = curNode.SuperNodes[(SuperNode)path.Nodes[pos + i]];
                        curNode = nextStep.To;
                        length += nextStep.Length - (nextStep.To?.SuperNodes[(SuperNode)path.Nodes[pos + i]]?.Length ?? 0);
                        pos += i;
                        break;
                    }                    
                }
                    
            }
            var finalPath = Path.Calculate(graph, curNode.Position, path.Target.Position);
            finalPath.Task.Wait();
            visited.AddRange(finalPath.Nodes);
            var walkedPath = new Path(path.Start, path.Target)
            {
                Nodes = visited,
                Length = length + finalPath.Length
            };
            return walkedPath;
        }
    }
}
