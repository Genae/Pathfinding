using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
