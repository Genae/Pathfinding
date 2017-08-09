using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding.Graphs;
using Pathfinding.Pathfinder;
using Pathfinding.Utils;
using Xunit;

namespace Pathfinding.Tests
{
    public class PerformanceTests : TestBase
    {

        [Fact]
        public void TestAStarMaceNoDeadEnds()
        {
            var img = LoadImage("Images/MAZE_40x20_DFS_no_deadends.png");
            var graph = GetGraphFromImage(img);
            graph.AddTier1Nodes(20);
            var path = Path.Calculate(graph, new Vector3I(12, 0, 12), new Vector3I(959, 0, 479));
            path.Task.Wait();
            var exactPath = (path as HighLevelPath)?.ExactPath ?? path;
            DrawPathToImage(img, exactPath.Nodes, Color.Red);
            var pathO = Path.Calculate(graph, new Vector3I(12, 0, 12), new Vector3I(959, 0, 479), true);
            pathO.Task.Wait();
            DrawPathToImage(img, pathO.Nodes, Color.Blue);
            SaveImage(img, @"C:\Test\Pathfinding\maceNoDeadEnds.bmp");
            Assert.True(path.Finished);
        }

        [Fact]
        public void TestAStarMaceWithDeadEnds()
        {
            var img = LoadImage("Images/maze_by_pannekaka.jpg");
            var graph = GetGraphFromImage(img);
            graph.AddTier1Nodes(20);
            //DrawNodesToImage(img, graph);
            var path = Path.Calculate(graph, new Vector3I(20, 0, 1185), new Vector3I(1563, 0, 25));
            path.Task.Wait();
            var exactPath = (path as HighLevelPath)?.ExactPath ?? path;
            DrawPathToImage(img, exactPath.Nodes, Color.Red);
            var pathO = Path.Calculate(graph, new Vector3I(20, 0, 1185), new Vector3I(1563, 0, 25), true);
            pathO.Task.Wait();
            DrawPathToImage(img, pathO.Nodes, Color.Blue);
            SaveImage(img, @"C:\Test\Pathfinding\maceWithDeadEnds.bmp");
            Assert.True(path.Finished);
        }

        [Fact]
        public void TestDeletingAndReaddingNodes()
        {
            var img = LoadImage("Images/MAZE_40x20_DFS_no_deadends.png");
            var graph = GetGraphFromImage(img);
            graph.AddTier1Nodes(20);
            var nodes = GetRandomNodes(graph, 20000);
            foreach (var node in nodes)
            {
                graph.RemoveNode(node);
            }
            foreach (var node in nodes)
            {
                graph.AddNode(node.x, node.y, node.z);
            }
            var path = Path.Calculate(graph, new Vector3I(12, 0, 12), new Vector3I(959, 0, 479));
            path.Task.Wait();
            var exactPath = (path as HighLevelPath)?.ExactPath ?? path;
            DrawPathToImage(img, exactPath.Nodes, Color.Red);
            var pathO = Path.Calculate(graph, new Vector3I(12, 0, 12), new Vector3I(959, 0, 479), true);
            pathO.Task.Wait();
            DrawPathToImage(img, pathO.Nodes, Color.Blue);
            SaveImage(img, @"C:\Test\Pathfinding\maceNoDeadEndsDelAdd.bmp");
            Assert.True(path.Finished);
        }


        private Vector3I[] GetRandomNodes(VoxelGraph graph, int amount)
        {
            var nodes = new HashSet<Node>();
            for (int i = 0; i < amount; i++)
            {
                nodes.Add(GetRandomNode(graph, nodes));
            }
            return nodes.Select(n => n.Position).ToArray();
        }
        private Node GetRandomNode(VoxelGraph graph, HashSet<Node> nodes)
        {
            var random = new Random();
            Node node;
            do
            {
                var size = graph.GetSize();
                var pos = new Vector3I(random.Next(size[0]), random.Next(size[1]), random.Next(size[2]));
                node = graph.GetNode(pos);
            } while (node == null || nodes.Contains(node));
            return node;
        }
    }
}
