namespace Fumbbl.Gamefinder.Model
{
    public class Coach
    {
        public HashSet<Team> _teams;
        public DateTime _lastEventTime = DateTime.Now;
        public string Name { get; set; } = String.Empty;

        public bool IsTimedOut => (DateTime.Now - _lastEventTime).TotalSeconds > 4;

        public int Id { get; set; }

        public Coach()
        {
            _teams = new();
        }

        public void Ping()
        {
            _lastEventTime = DateTime.Now;
        }

        public void Add(Team team)
        {
            _teams.Add(team);
        }

        public IEnumerable<Team> GetTeams()
        {
            return _teams;
        }

        public override string ToString()
        {
            return $"Coach({Name})";
        }
    }
}