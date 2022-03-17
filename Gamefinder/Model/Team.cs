using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Team : IEquatable<Team>
    {
        public Coach Coach { get; init; }
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }

        private readonly ConcurrentHashSet<BasicMatch> _matches;

        public Team(Coach coach)
        {
            Coach = coach;
            _matches = new();
        }

        public void Add(Match m)
        {
            _matches.Add(m);
        }

        public void Remove(BasicMatch m)
        {
            _matches.TryRemove(m);
        }

        internal bool IsOpponentAllowed(Team opponent)
        {
            return !Equals(Coach, opponent.Coach);
        }

        internal IEnumerable<BasicMatch> GetMatches()
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
            return other is not null && other is Team team && Equals(team);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Match", Id);
        }
    }
}
