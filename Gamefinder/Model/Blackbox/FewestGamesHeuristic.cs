namespace Fumbbl.Gamefinder.Model.Blackbox
{
    public class FewestGamesHeuristic : ISchedulerHeuristic
    {
        public List<int> GenerateProcessingOrder(Dictionary<int, List<BasicMatch>> matches)
        {
            var list = matches.ToList();
            list.Sort((a, b) => a.Value.Count - b.Value.Count);
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
