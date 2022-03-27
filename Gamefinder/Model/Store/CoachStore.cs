using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model.Store
{
    internal class CoachStore
    {
        private readonly ILogger<MatchGraph> _logger;
        private readonly ConcurrentHashSet<Coach> _coaches;

        public CoachStore(ILogger<MatchGraph> logger)
        {
            _logger = logger;
            _coaches = new();
        }

        internal void Clear()
        {
            _coaches.Clear();
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