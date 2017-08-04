namespace Pathfinding.Graphs
{
    public class Edge
    {
        public Node To;
        public float Length;

        public Edge(Node to, float length)
        {
            To = to;
            Length = length;
        }
    }

    public class SuperNodeConnection : Edge
    {
        public SuperNode SuperNode;

        public SuperNodeConnection(Node to, SuperNode superNode, float length) : base(to, length)
        {
            SuperNode = superNode;
        }
    }

    public class SuperNodeEdge : Edge
    {
        public Node ConnectorNode;
        public SuperNodeEdge(Node to, float length, Node via) : base(to, length)
        {
            ConnectorNode = via;
        }
    }
}