using System.Linq;
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
            var n2 = n1.Neighbours.Keys.FirstOrDefault(n => n.Position.z.Equals(1));
            Assert.NotNull(n2);
            var n3 = n2.Neighbours.Keys.FirstOrDefault(n => n.Position.z.Equals(2));
            Assert.NotNull(n3);
            var n4 = n3.Neighbours.Keys.FirstOrDefault(n => n.Position.z.Equals(3));
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
    }
}
