using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Team : IEquatable<Team>
    {
        public Coach Coach { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }

        public string Division { get; set; } = string.Empty;
        public int TeamValue { get; set; }
        public string Roster { get; set; } = string.Empty;
        public int RosterLogo32 { get; set; }
        public int RosterLogo64 { get; set; }
        public int Season { get; set; }
        public int SeasonGames { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public string Status { get; set; } = String.Empty;
        public bool AllowCrossLeagueMatches { get; set; }

        public bool IsActive => string.Equals(Status, "Active");

        public Tournament? Tournament { get; set; }
        public int Ruleset { get; set; }
        public TvLimit TvLimit { get; set; } = new();

        public Team(Coach coach)
        {
            Coach = coach;
        }

        internal bool IsOpponentAllowed(Team opponent)
        {
            if (!string.Equals(opponent.Division, this.Division))
            {
                return false;
            }

            if (Equals(Coach, opponent.Coach))
            {
                return false;
            }

            if (!Coach.CanLfg || !opponent.Coach.CanLfg)
            {
                return false;
            }

            if (!IsActive || !opponent.IsActive)
            {
                return false;
            }

            if (TeamValue == 0 || opponent.TeamValue == 0)
            {
                return false;
            }

            if (Tournament?.Id < 0 || opponent.Tournament?.Id < 0)
            {
                return false;
            }

            if (Tournament != null && Tournament.Id > 0 && !Tournament.ValidOpponent(opponent.Id))
            {
                return false;
            }

            if (opponent.Tournament != null && opponent.Tournament.Id > 0 && !opponent.Tournament.ValidOpponent(Id))
            {
                return false;
            }

            if (Ruleset != opponent.Ruleset)
            {
                return false;
            }

            if (LeagueId != opponent.LeagueId)
            {
                if (!AllowCrossLeagueMatches || !opponent.AllowCrossLeagueMatches)
                {
                    return false;
                }
            }

            if (!TvLimit.IsWithinRange(opponent.TeamValue) || !opponent.TvLimit.IsWithinRange(TeamValue))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"Team({Name})";
        }

        public bool Equals(Team? other)
        {
            return other is not null && this.Id == other.Id;
        }

        public override bool Equals(object? other)
        {
            return other is not null && other is Team team && Equals(team);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Match", Id);
        }

        internal void Update(Team team)
        {
            Coach = team.Coach;
            Name = team.Name;
            Division = team.Division;
            TeamValue = team.TeamValue;
            Roster = team.Roster;
            RosterLogo32 = team.RosterLogo32;
            RosterLogo64 = team.RosterLogo64;
            Season = team.Season;
            SeasonGames = team.SeasonGames;
            LeagueId = team.LeagueId;
            LeagueName = team.LeagueName;
            Status = team.Status;
            Tournament = team.Tournament;
            AllowCrossLeagueMatches = team.AllowCrossLeagueMatches;
        }
    }
}
