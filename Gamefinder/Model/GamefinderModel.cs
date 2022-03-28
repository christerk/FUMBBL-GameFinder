using Fumbbl.Gamefinder.Model.Event;

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
            if (e is MatchUpdatedArgs matchEvent)
            {
                var coach1 = matchEvent?.Match?.Team1.Coach;
                var coach2 = matchEvent?.Match?.Team1.Coach;
                if (coach1 != null && coach2 != null)
                {
                    _matchGraph.DialogManager.Remove(coach1);
                    _matchGraph.DialogManager.Remove(coach2);
                }
            }
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
