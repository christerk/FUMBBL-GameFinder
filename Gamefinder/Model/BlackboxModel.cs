namespace Fumbbl.Gamefinder.Model
{
    public class BlackboxModel
    {
        private readonly MatchGraph _matchGraph;
        private List<BasicMatch> _matches;

        public MatchGraph Graph => _matchGraph;

        public BlackboxModel(MatchGraph matchGraph)
        {
            _matchGraph = matchGraph;
            _matches = new List<BasicMatch>();
        }

        public IEnumerable<BasicMatch>? GenerateRound()
        {
            var possibleMatches = GetPossibleMatches();

            var bestMatches = GenerateBestMatches(possibleMatches.ToList());

            return bestMatches;
        }

        private List<BasicMatch>? GenerateBestMatches(List<BasicMatch> possibleMatches)
        {
            var maxSuitability = -1;
            List<BasicMatch>? bestMatches = null;
            for (int i=0; i < possibleMatches.Count; i++)
            {
                var potentialMatch = possibleMatches[i];
                var remainingMatches = possibleMatches.Where(m => !CoachesCollide(potentialMatch, m)).ToList();
                var bestSubset = GenerateBestMatches(remainingMatches);
                if (bestSubset is not null)
                {
                    var suitability = bestSubset.Sum(m => getSuitability(m)) + getSuitability(potentialMatch);

                    if (suitability > maxSuitability)
                    {
                        bestMatches = bestSubset;
                        bestMatches.Add(potentialMatch);
                    }
                }
                else
                {
                    bestMatches = new() { potentialMatch };
                }
            }

            return bestMatches;
        }

        private bool CoachesCollide(BasicMatch potentialMatch, BasicMatch m)
        {
            return potentialMatch.Team1.Coach.Equals(m.Team1.Coach)
                || potentialMatch.Team1.Coach.Equals(m.Team2.Coach)
                || potentialMatch.Team2.Coach.Equals(m.Team1.Coach)
                || potentialMatch.Team2.Coach.Equals(m.Team2.Coach);
        }

        private IEnumerable<BasicMatch> GetPossibleMatches()
        {
            HashSet<(Coach, Coach)> processed = new();
            foreach (var coach in _matchGraph.GetCoaches().Where(c => ActivatedForBlackbox(c)))
            {
                var coachMatches = _matchGraph.GetMatches(coach).Where(m => AllowedForBlackbox(m));

                var opponents = coachMatches.GroupBy(m => m.GetOpponent(coach));
                foreach (var opponent in opponents)
                {
                    if (opponent.Key is not null && !processed.Contains((opponent.Key, coach)))
                    {
                        var bestMatch = opponent.MaxBy(m => getSuitability(m));
                        if (bestMatch is not null)
                        {
                            yield return bestMatch;
                        }
                    }
                }
            }
        }

        private bool AllowedForBlackbox(BasicMatch m)
        {
            return true;
        }

        private bool ActivatedForBlackbox(Coach c)
        {
            return true;
        }

        private int getSuitability(BasicMatch match)
        {
            return getSuitability(match.Team1, match.Team2);
        }

        private int getSuitability(Team team1, Team team2)
        {
            return 100;
        }
    }
}
