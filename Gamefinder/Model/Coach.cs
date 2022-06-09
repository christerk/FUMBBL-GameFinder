using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Coach : IEquatable<Coach>, IKeyedItem<int>
    {
        public const int COACH_TIMEOUT = 10;
        public string Name { get; set; } = String.Empty;
        public int Id { get; set; }

        public IEnumerable<int> RecentOpponents { get; set; } = Enumerable.Empty<int>();
        public bool Locked { get; private set; } = false;

        public int Key => Id;

        public string Rating { get; set; } = String.Empty;
        public bool CanLfg { get; set; }

        public Coach()
        {
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