using Fumbbl.Api;
using Fumbbl.Gamefinder.Convert;

namespace Fumbbl.Gamefinder.Model.Cache
{
    public class CoachCache : FlushableCache<int, Coach>
    {
        private readonly FumbblApi _fumbbl;
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

            return coach;
        }
    }
}