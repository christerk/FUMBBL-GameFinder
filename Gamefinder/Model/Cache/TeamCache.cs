using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;

namespace Fumbbl.Gamefinder.Model.Cache
{
    public class TeamCache
    {
        private readonly FumbblApi _fumbbl;
        public TeamCache(FumbblApi fumbbl)
        {
            _fumbbl = fumbbl;
        }

        public async Task<IEnumerable<Team>> GetTeams(Coach coach)
        {
            var apiTeams = await _fumbbl.Coach.TeamsAsync(coach.Id);

            return GetTeams(coach, apiTeams);
        }

        public async Task<IEnumerable<Team>> GetLfgTeams(Coach coach)
        {
            var apiTeams = await _fumbbl.Coach.LfgTeamsAsync(coach.Id);

            return GetTeams(coach, apiTeams);
        }

        private static IEnumerable<Team> GetTeams(Coach coach, Api.DTO.CoachTeams? apiTeams)
        {
            List<Team> teams = new();
            if (apiTeams is not null)
            {
                if (apiTeams.RecentOpponents is not null)
                {
                    coach.RecentOpponents = new List<int>(apiTeams.RecentOpponents);
                }
                foreach (var apiTeam in apiTeams.Teams.Where(t => string.Equals(t.IsLfg, "Yes") && string.Equals(t.Status, "Active")))
                {
                    var team = apiTeam.ToModel(coach);
                    if (team is not null)
                    {
                        teams.Add(team);
                    }
                }
            }
            return teams;
        }
    }
}
