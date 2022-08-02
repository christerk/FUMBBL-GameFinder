namespace Fumbbl.Gamefinder.Model.Blackbox
{
    public interface ISchedulerHeuristic
    {
        void PreProcess(Dictionary<int, List<BasicMatch>> matches);
        List<int> GenerateProcessingOrder(Dictionary<int, List<BasicMatch>> matches);
    }
}
