using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding.Agents;
using Xunit;
using Pathfinding.Graphs;
using Pathfinding.Utils;
using Pathfinding.Pathfinder;

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

            var n1 = graph.GetNode(new Vector3I(0, 0, 0));
            Assert.NotNull(n1);
            var n2 = n1.GetNeighbours().FirstOrDefault(n => n.To.Position.z.Equals(1))?.To;
            Assert.NotNull(n2);
            var n3 = n2.GetNeighbours().FirstOrDefault(n => n.To.Position.z.Equals(2))?.To;
            Assert.NotNull(n3);
            var n4 = n3.GetNeighbours().FirstOrDefault(n => n.To.Position.z.Equals(3))?.To;
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

            var path = Path.Calculate(graph, new Vector3I(0, 0, 0), new Vector3I(0, 1, 3));
            path.Task.Wait();
            Assert.True(path.Finished);
        }

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
        public void RemovingNodesDoesntBreakTheGraph()
        {
            var graph = new VoxelGraph();
            for (var i = 0; i < 200; i++)
            {
                for (var j = 0; j < 200; j++)
                {
                    graph.AddNode(i, 0, j);
                }
            }
            graph.AddTier1Nodes(10);
            graph.RemoveNode(new Vector3I(1, 0, 0));
            var path = Path.Calculate(graph, new Vector3I(0, 0, 0), new Vector3I(199, 0, 199));
            path.Task.Wait();
            Assert.True(path.Finished);
            Assert.Equal(Math.Sqrt(2)*199, path.Length, 2);
        }

        [Fact]
        public void RemovingNodesDoesActuallyRemoveThem()
        {
            var graph = new VoxelGraph();
            for (var i = 0; i < 300; i++)
            {
                graph.AddNode(i, 0, 0);
            }
            graph.AddTier1Nodes(10);
            graph.RemoveNode(new Vector3I(1, 0, 0));
            var path = Path.Calculate(graph, new Vector3I(0, 0, 0), new Vector3I(299, 0, 0), true);
            path.Task.Wait();
            Assert.True(path.Finished);
            Assert.Equal(0, path.Length);

            var pathHl = Path.Calculate(graph, new Vector3I(0, 0, 0), new Vector3I(299, 0, 0));
            pathHl.Task.Wait();
            Assert.True(pathHl.Finished);
            Assert.Equal(0, pathHl.Length);
        }

        [Fact]
        public void RemovingNodesReplansSupernodeConnections()
        {
            /*
             *      #
             *      #
             *        #
             *      #   X
             *      #   #
             *      #   #
             *        #
             * 
             * 
             * 
             */
            var graph = new VoxelGraph();
            graph.AddNode(5, 5, 0);
            graph.AddNode(5, 6, 0);
            graph.AddNode(6, 7, 0);
            graph.AddNode(5, 8, 0);
            graph.AddNode(5, 9, 0);
            graph.AddNode(5, 10, 0);
            graph.AddNode(6, 11, 0);
            graph.AddNode(7, 10, 0);
            graph.AddNode(7, 9, 0);
            graph.AddNode(7, 8, 0);
            graph.AddTier1Nodes(10);

            Assert.Equal(3.83, graph.GetNode(new Vector3I(7, 8, 0)).SuperNodes.Values.First().Length, 2);
            Assert.Equal(4.83, graph.GetNode(new Vector3I(7, 9, 0)).SuperNodes.Values.First().Length, 2);

            graph.RemoveNode(new Vector3I(7, 8, 0));

            Assert.Equal(9.66, graph.GetNode(new Vector3I(7, 9, 0)).SuperNodes.Values.First().Length, 2);

        }


        [Fact]
        public void RemovingNodesSeperatesConnections()
        {
            /*
             *      #
             *      #
             *      x
             *      #   
             *      #  
             *      #   
             *       
             * 
             * 
             * 
             */
            var graph = new VoxelGraph();
            graph.AddNode(5, 5, 0);
            graph.AddNode(5, 6, 0);
            graph.AddNode(5, 7, 0);
            graph.AddNode(5, 8, 0);
            graph.AddNode(5, 9, 0);
            graph.AddNode(5, 10, 0);
            graph.AddTier1Nodes(10);


            var superNode = graph.GetT1Nodes().First() as SuperNode;
            Assert.NotNull(superNode);
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 5, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 6, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 7, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 8, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 9, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 10, 0)).SuperNodes.Keys.ToList());

            graph.RemoveNode(new Vector3I(5, 7, 0));

            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 5, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode, graph.GetNode(new Vector3I(5, 6, 0)).SuperNodes.Keys.ToList());

            Assert.DoesNotContain(superNode, graph.GetNode(new Vector3I(5, 8, 0)).SuperNodes.Keys.ToList());
            Assert.DoesNotContain(superNode, graph.GetNode(new Vector3I(5, 9, 0)).SuperNodes.Keys.ToList());
            Assert.DoesNotContain(superNode, graph.GetNode(new Vector3I(5, 10, 0)).SuperNodes.Keys.ToList());

            Assert.Contains(graph.GetNode(new Vector3I(5, 5, 0)), superNode.ChildNodes);
            Assert.Contains(graph.GetNode(new Vector3I(5, 6, 0)), superNode.ChildNodes);
            Assert.Equal(2, superNode.ChildNodes.Count);

            Assert.Equal(2, graph.GetT1Nodes().Count());
            var superNode2 = graph.GetT1Nodes().Last();
            Assert.Contains(superNode2, graph.GetNode(new Vector3I(5, 8, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode2, graph.GetNode(new Vector3I(5, 9, 0)).SuperNodes.Keys.ToList());
            Assert.Contains(superNode2, graph.GetNode(new Vector3I(5, 10, 0)).SuperNodes.Keys.ToList());

        }

        #region util
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

        private void DrawPathToImage(Bitmap img, IEnumerable<Node> nodes, Color c)
        {
            foreach (var pathNode in nodes)
            {
                img.SetPixel((int)pathNode.Position.x, (int)pathNode.Position.z, c);
            }
        }

        private void DrawNodesToImage(Bitmap img, VoxelGraph graph)
        {
            var rand = new Random();
            var colors = graph.GetT1Nodes().ToDictionary(n => n, n => Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255)));

            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    var node = graph.GetNode(new Vector3I(x, 0, y));
                    if(node != null && node.SuperNodes.Count > 0)
                        img.SetPixel(x, y, colors[node.GetClosestSuperNode()]);
                }
            }
        }

        private void SaveImage(Bitmap img, string output)
        {
            if (!System.IO.Directory.GetParent(output).Exists)
            {
                System.IO.Directory.GetParent(output).Create();
            }
            img.Save(output);
        }
        #endregion
    }
}
