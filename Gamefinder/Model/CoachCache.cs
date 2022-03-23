using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;

namespace Fumbbl.Gamefinder.Model
{
    public class CoachCache : FlushableCache<int, Coach>
    {
        private FumbblApi _fumbbl;
        public CoachCache(FumbblApi fumbbl)
        {
            _fumbbl = fumbbl;
        }

        public override async Task<Coach?> CreateAsync(int coachId)
        {
            var apiCoach = await _fumbbl.Coach.GetAsync(coachId);

            if (apiCoach == null)
            {
                return null;
            }

            var coach = apiCoach.ToModel();

            var apiTeams = await _fumbbl.Coach.TeamsAsync(coachId);

            if (apiTeams is not null)
            {
                foreach (var apiTeam in apiTeams.Teams.Where(t => string.Equals(t.IsLfg, "Yes") && string.Equals(t.Status, "Active")))
                {
                    var team = apiTeam.ToModel(coach);
                    if (team is not null)
                    {
                        coach.Add(team);
                    }
                }
            }

            return coach;
        }
    }
}