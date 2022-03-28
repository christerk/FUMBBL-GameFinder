using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    public class Coach : IEquatable<Coach>, IKeyedItem<int>
    {
        public string Name { get; set; } = String.Empty;
        public int Id { get; set; }

        public bool Locked { get; private set; } = false;

        public int Key => Id;

        public string Rating { get; set; } = String.Empty;

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