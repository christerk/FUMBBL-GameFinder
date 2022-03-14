using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Team
    {
        public Coach Coach { get; init; }
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }

        private readonly ConcurrentHashSet<Match> _matches;

        public Team(MatchGraph graph, Coach coach)
        {
            Coach = coach;
            _matches = new();
            graph.AddTeamToCoach(this, coach);
        }

        public void Add(Match m)
        {
            _matches.Add(m);
        }

        public void Remove(Match m)
        {
            _matches.TryRemove(m);
        }

        internal bool IsOpponentAllowed(Team opponent)
        {
            return !Equals(Coach, opponent.Coach);
        }

        internal IEnumerable<Match> GetMatches()
        {
            return _matches;
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
            return other is not null && other is Team && Equals((Team)other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Match", Id);
        }
    }
}
