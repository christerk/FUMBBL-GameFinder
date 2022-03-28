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

        public async void ActivateAsync(Coach activatingCoach, IEnumerable<Team> activatingTeams)
        {
            var coachExists = await _matchGraph.Contains(activatingCoach);
            if (!coachExists)
            {
                await _matchGraph.AddAsync(activatingCoach);
            }
            _matchGraph.Ping(activatingCoach);

            var graphTeams = (await _matchGraph.GetTeamsAsync(activatingCoach)).ToHashSet();
            foreach (var team in activatingTeams)
            {
                if (!graphTeams.Contains(team))
                {
                    await _matchGraph.AddAsync(team);
                }
                else
                {
                    var graphTeam = graphTeams.Where(t => t.Equals(team)).First();
                    graphTeam.Update(team);
                }
            }
            foreach (var team in graphTeams)
            {
                if (!activatingTeams.Contains(team))
                {
                    await _matchGraph.RemoveAsync(team);
                }
            }
        }
    }
}
