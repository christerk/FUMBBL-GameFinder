namespace Fumbbl.Gamefinder.Model
{
    public class BlackboxContext : ISchedulingContext
    {
        public bool IsOpponentAllowed(Team team, Team opponent)
        {
            if (team.LfgMode == LfgMode.Open || opponent.LfgMode == LfgMode.Open)
            {
                return false;
            }

            if (team.Coach.Equals(opponent.Coach))
            {
                return false;
            }

            if (!team.IsActive || !opponent.IsActive)
            {
                return false;
            }

            if (!string.Equals(opponent.Division, team.Division) || !string.Equals(team.Division, "Competitive"))
            {
                return false;
            }

            if (!team.Coach.CanLfg || !opponent.Coach.CanLfg)
            {
                return false;
            }

            if (team.SchedulingTeamValue == 0 || opponent.SchedulingTeamValue == 0)
            {
                return false;
            }

            if (!team.TvLimit.IsWithinRange(opponent.SchedulingTeamValue) || !opponent.TvLimit.IsWithinRange(team.SchedulingTeamValue))
            {
                return false;
            }

            if ((team.Season == 1) != (opponent.Season == 1))
            {
                // Real first season check
                return false;
            }

            if (
                Math.Abs(team.CurrentTeamValue - opponent.CurrentTeamValue) > 350000
                && team.Season == 1 && opponent.Season == 1
            )
            {
                return false;
            }

            if (team.Tournament?.Id < 0 || opponent.Tournament?.Id < 0)
            {
                return false;
            }

            if (team.Tournament != null && team.Tournament.Id > 0 && !team.Tournament.ValidOpponent(opponent.Id))
            {
                return false;
            }

            if (opponent.Tournament != null && opponent.Tournament.Id > 0 && !opponent.Tournament.ValidOpponent(team.Id))
            {
                return false;
            }

            if (team.RulesetId != opponent.RulesetId)
            {
                return false;
            }

            if (team.LeagueId != opponent.LeagueId)
            {
                if (!team.AllowCrossLeagueMatches || !opponent.AllowCrossLeagueMatches)
                {
                    return false;
                }
            }

            return true;
        }
    }
}