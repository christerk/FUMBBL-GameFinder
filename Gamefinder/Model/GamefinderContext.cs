namespace Fumbbl.Gamefinder.Model
{
    public class GamefinderContext : ISchedulingContext
    {
        public bool IsOpponentAllowed(Team team, Team opponent)
        {
            if (!string.Equals(opponent.Division, team.Division))
            {
                return false;
            }

            if (Equals(team.Coach, opponent.Coach))
            {
                return false;
            }

            if (team.LfgMode == LfgMode.Strict || opponent.LfgMode == LfgMode.Strict)
            {
                return false;
            }


            if (team.Competitive)
            {
                if (!team.Coach.CanLfg || !opponent.Coach.CanLfg)
                {
                    return false;
                }

                if (team.LastOpponent == opponent.Coach.Id || opponent.LastOpponent == team.Coach.Id)
                {
                    return false;
                }

                if (team.Coach.RecentOpponents.Contains(opponent.Coach.Id) || opponent.Coach.RecentOpponents.Contains(team.Coach.Id))
                {
                    return false;
                }

                if (!team.TvLimit.IsWithinRange(opponent.SchedulingTeamValue) || !opponent.TvLimit.IsWithinRange(team.SchedulingTeamValue))
                {
                    return false;
                }
            }

            if (!team.IsActive || !opponent.IsActive)
            {
                return false;
            }

            if (team.SchedulingTeamValue == 0 || opponent.SchedulingTeamValue == 0)
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