namespace Fumbbl.Gamefinder.Model.Blackbox
{
    public class HighestSuitabilityHeuristic : ISchedulerHeuristic
    {
        public List<int> GenerateProcessingOrder(Dictionary<int, List<BasicMatch>> matches)
        {
            var list = matches.ToList();
            list.Sort((a, b) => b.Value[0].Suitability ?? 0 - a.Value[0].Suitability ?? 0);
            return list.Select(a => a.Key).ToList();
        }

        public void PreProcess(Dictionary<int, List<BasicMatch>> matches)
        {
            foreach (var pair in matches)
            {
                var list = pair.Value;
                list.Sort((a, b) => (b.Suitability ?? 0) - (a.Suitability ?? 0));
                matches[pair.Key] = list;
            }
        }
    }
}
