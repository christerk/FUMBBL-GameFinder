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

        public async Task GenerateRound()
        {
            _matches = await _matchGraph.GetMatchesAsync();
            MarkPrioritizedGames();
        }

        private void MarkPrioritizedGames()
        {
            // Reset prioritized matches
            foreach (var match in _matches)
            {
                match.BlackboxPrioritized = false;
            }


        }
    }
}
