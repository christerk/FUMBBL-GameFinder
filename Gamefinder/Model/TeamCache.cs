using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;

namespace Fumbbl.Gamefinder.Model
{
    public class TeamCache
    {
        private readonly FumbblApi _fumbbl;
        public TeamCache(FumbblApi fumbbl)
        {
            _fumbbl = fumbbl;
        }

        public async IAsyncEnumerable<Team> GetTeams(Coach coach)
        {
            var apiTeams = await _fumbbl.Coach.TeamsAsync(coach.Id);

            if (apiTeams is not null)
            {
                foreach (var apiTeam in apiTeams.Teams.Where(t => string.Equals(t.IsLfg, "Yes") && string.Equals(t.Status, "Active")))
                {
                    var team = apiTeam.ToModel(coach);
                    if (team is not null)
                    {
                        yield return team;
                    }
                }
            }
        }
    }
}
