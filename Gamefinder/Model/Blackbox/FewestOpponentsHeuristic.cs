namespace Fumbbl.Gamefinder.Model.Blackbox
{
    public class FewestOpponentsHeuristic : ISchedulerHeuristic
    {
        public List<int> GenerateProcessingOrder(Dictionary<int, List<BasicMatch>> matches)
        {
            var list = matches.ToList();
            list.Sort((a, b) => CountCoaches(a.Value) - CountCoaches(b.Value));
            return list.Select(a => a.Key).ToList();
        }

        private int CountCoaches(List<BasicMatch> matches)
        {
            HashSet<int> coaches = new();
            foreach (var match in matches)
            {
                coaches.Add(match.Team1.Coach.Id);
                coaches.Add(match.Team2.Coach.Id);
            }
            return coaches.Count;
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
