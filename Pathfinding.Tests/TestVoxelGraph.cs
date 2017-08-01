using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace Pathfinding.Tests
{
    public class TestVoxelGraph
    {
        [Fact]
        public void NodesAddedConnectCorrectly()
        {
            var graph = new VoxelGraph();
            graph.AddNode(0, 0, 0);
            graph.AddNode(0, 0, 1);
            graph.AddNode(0, 1, 3);
            graph.AddNode(1, 0, 2);

            var n1 = graph.GetNode(new Vector3(0, 0, 0));
            Assert.NotNull(n1);
            var n2 = n1.Neighbours.FirstOrDefault(n => n.To.Position.z.Equals(1))?.To;
            Assert.NotNull(n2);
            var n3 = n2.Neighbours.FirstOrDefault(n => n.To.Position.z.Equals(2))?.To;
            Assert.NotNull(n3);
            var n4 = n3.Neighbours.FirstOrDefault(n => n.To.Position.z.Equals(3))?.To;
            Assert.NotNull(n4);
        }

        [Fact]
        public void GraphCanSupportAStar()
        {
            var graph = new VoxelGraph();
            graph.AddNode(0, 0, 0);
            graph.AddNode(0, 0, 1);
            graph.AddNode(0, 1, 3);
            graph.AddNode(1, 0, 2);

            var path = Path.Calculate(graph, new Vector3(0, 0, 0), new Vector3(0, 1, 3));
            path.Task.Wait();
            Assert.True(path.Finished);
        }

        [Fact]
        public void TestAStarMaceNoDeadEnds()
        {
            var img = LoadImage("Images/MAZE_40x20_DFS_no_deadends.png");
            var graph = GetGraphFromImage(img);
            var path = Path.Calculate(graph, new Vector3(12, 0, 12), new Vector3(959, 0, 479));
            path.Task.Wait();
            DrawPathToImage(img, path, @"C:\Test\Pathfinding\maceNoDeadEnds.bmp");
            Assert.True(path.Finished);
        }

        [Fact]
        public void TestAStarMaceWithDeadEnds()
        {
            var img = LoadImage("Images/maze_by_pannekaka.jpg");
            var graph = GetGraphFromImage(img);
            var path = Path.Calculate(graph, new Vector3(20, 0, 1185), new Vector3(1563, 0, 25));
            path.Task.Wait();
            DrawPathToImage(img, path, @"C:\Test\Pathfinding\maceWithDeadEnds.bmp");
            Assert.True(path.Finished);
        }


        private Bitmap LoadImage(string path)
        {
            var src = new Bitmap(Environment.CurrentDirectory + "/" + path);
            var newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0, src.Width, src.Height);
            }
            return newBmp;
        }

        private VoxelGraph GetGraphFromImage(Bitmap img)
        {
            var graph = new VoxelGraph();
            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    if (img.GetPixel(x, y).R > 200)
                    {
                        graph.AddNode(x, 0, y);
                    }
                }
            }
            return graph;
        }

        private void DrawPathToImage(Bitmap img, Path path, string output)
        {
            foreach (var pathNode in path.Nodes)
            {
                img.SetPixel((int)pathNode.Position.x, (int)pathNode.Position.z, Color.Red);
            }
            if (!Directory.GetParent(output).Exists)
            {
                Directory.GetParent(output).Create();
            }
            img.Save(output);
        }
    }
}
