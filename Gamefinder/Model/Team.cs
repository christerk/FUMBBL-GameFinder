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

        public Team(Coach coach)
        {
            Coach = coach;
        }

        internal bool IsOpponentAllowed(Team opponent)
        {
            return !Equals(Coach, opponent.Coach);
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
        }
    }
}
