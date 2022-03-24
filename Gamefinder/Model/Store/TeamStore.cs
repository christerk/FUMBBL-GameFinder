using ConcurrentCollections;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model.Store
{
    internal class TeamStore
    {
        private readonly ConcurrentHashSet<Team> _teams;
        private readonly ConcurrentDictionary<Coach, ConcurrentHashSet<Team>> _coachTeams;

        public TeamStore()
        {
            _teams = new();
            _coachTeams = new();
        }

        internal IEnumerable<Team> GetTeams()
        {
            return _teams;
        }

        internal IEnumerable<Team> GetTeams(Coach coach)
        {
            if (_coachTeams.ContainsKey(coach))
            {
                return _coachTeams[coach];
            }
            return Enumerable.Empty<Team>();
        }

        internal bool Contains(Team team)
        {
            return _teams.Contains(team);
        }

        internal bool Add(Team team)
        {
            Add(team.Coach, team);
            return _teams.Add(team);
        }

        internal bool Remove(Team team)
        {
            Remove(team.Coach, team);
            return _teams.TryRemove(team);
        }

        private bool Remove(Coach coach, Team team)
        {
            if (_coachTeams.ContainsKey(coach))
            {
                return _coachTeams[coach].TryRemove(team);
            }
            return false;
        }

        private bool Add(Coach coach, Team team)
        {
            if (!_coachTeams.ContainsKey(coach))
            {
                _coachTeams.TryAdd(coach, new());
            }
            return _coachTeams[coach].Add(team);
        }
    }
}