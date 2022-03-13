namespace Fumbbl.Gamefinder.Model
{
    public class Team
    {
        public Coach Coach { get; init; }
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }

        private readonly HashSet<Match> _matches;

        public Team(Coach coach)
        {
            Coach = coach;
            _matches = new();
            Coach.Add(this);
        }

        public void Add(Match m)
        {
            _matches.Add(m);
        }

        public void Remove(Match m)
        {
            _matches.Remove(m);
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
    }
}
