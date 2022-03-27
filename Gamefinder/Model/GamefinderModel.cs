namespace Fumbbl.Gamefinder.Model
{
    public class GamefinderModel
    {
        private readonly MatchGraph _matchGraph;

        public MatchGraph Graph => _matchGraph;

        public GamefinderModel(MatchGraph matchGraph)
        {
            _matchGraph = matchGraph;
            _matchGraph.MatchLaunched += MatchLaunched;
            _matchGraph.Start();
        }

        private void MatchLaunched(object? sender, EventArgs e)
        {
            // Call to FUMBBL API to start the game

            // Tell MatchGraph which FFB Game ID needs to be redirected to
        }

        public async void ActivateAsync(Coach coach, IAsyncEnumerable<Team> teams)
        {
            // If coach is not in graph already

            //await _matchGraph.AddAsync(coach);


            // add to graph and update teams

            await foreach (var team in teams)
            {
                // if team exists, update

                // else add
                await Graph.AddAsync(team);
            }

            //await _matchGraph.RemoveAsync(coach);
            //await _matchGraph.AddAsync(coach);
        }
    }
}
