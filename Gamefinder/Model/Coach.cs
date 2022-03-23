using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Coach : IEquatable<Coach>, IKeyedItem<int>
    {
        public ConcurrentHashSet<Team> _teams;
        public string Name { get; set; } = String.Empty;
        public int Id { get; set; }

        public DateTime _lastEventTime = DateTime.Now;

        public bool IsTimedOut => (DateTime.Now - _lastEventTime).TotalSeconds > 4;

        public bool Locked { get; private set; } = false;

        public int Key => Id;

        public string Rating { get; set; }

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

        public void Lock()
        {
            Locked = true;
        }

        public void Unlock()
        {
            Locked = false;
        }

        public bool Equals(Coach? other)
        {
            return other is not null && this.Id == other.Id;
        }

        public override bool Equals(object? other)
        {
            return other is not null && other is Coach coach && Equals(coach);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Coach", Id);
        }
    }
}