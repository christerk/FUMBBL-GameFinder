using QuickGraph;

namespace GamefinderVisualizer.Models
{
    public class GamefinderGraph : BidirectionalGraph<DataVertex, DataEdge>
    {
        private readonly object _lockObject = new();


        public void Generate(GamefinderGraphArea area)
        {
            lock (_lockObject)
            {
                area.GenerateGraph(true, true);
            }
        }

        public override bool RemoveEdge(DataEdge e)
        {
            lock (_lockObject)
            {
                return base.RemoveEdge(e);
            }
        }

        public override bool AddEdge(DataEdge e)
        {
            lock (_lockObject)
            {
                return base.AddEdge(e);
            }
        }

        public override bool AddVertex(DataVertex v)
        {
            lock (_lockObject)
            {
                return base.AddVertex(v);
            }
        }

        public override bool RemoveVertex(DataVertex v)
        {
            lock (_lockObject)
            {
                return base.RemoveVertex(v);
            }
        }
    }
}
