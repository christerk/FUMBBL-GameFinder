using GraphX.Common.Models;

namespace GamefinderVisualizer.Models
{
    public class DataEdge : EdgeBase<DataVertex>
    {
        public string? State { get; set; }
        public bool IsMatch { get; set; }
        public DataEdge(DataVertex source, DataVertex target, double weight = 1)
            : base(source, target, weight)
        {
        }

        public override string ToString()
        {
            return State ?? string.Empty;
        }
    }
}