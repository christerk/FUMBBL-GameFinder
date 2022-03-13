using GraphX.Common.Models;

namespace GamefinderVisualizer.Models
{
    public class DataVertex : VertexBase
    {
        public enum VertexType
        {
            Coach,
            Team
        }

        public VertexType VType { get; set; } = DataVertex.VertexType.Team;
        public int SortId { get; set; } = 0;
        public string Label { get; set; } = string.Empty;
        public bool IsCoach => VType == VertexType.Coach;
        public override string ToString()
        {
            return Label;
        }
    }
}