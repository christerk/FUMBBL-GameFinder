using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model
{
    internal class CoachStore
    {
        private readonly ConcurrentHashSet<Coach> _coaches;

        public CoachStore()
        {
            _coaches = new();
        }

        internal IEnumerable<Coach> GetCoaches()
        {
            return _coaches;
        }

        internal bool Contains(Coach coach)
        {
            return _coaches.Contains(coach);
        }

        internal bool Add(Coach coach)
        {
            return _coaches.Add(coach);
        }

        internal bool Remove(Coach coach)
        {
            return _coaches.TryRemove(coach);
        }
    }
}