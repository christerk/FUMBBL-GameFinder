using System.Collections.Concurrent;
using ConcurrentCollections;

namespace Fumbbl.Gamefinder.Model.Store
{
    internal class MatchStore
    {
        private readonly ILogger<MatchGraph> _logger;
        private readonly ConcurrentHashSet<BasicMatch> _matches;
        private readonly ConcurrentDictionary<Team, ConcurrentHashSet<BasicMatch>> _teamMatches;

        public MatchStore(ILogger<MatchGraph> logger)
        {
            _logger = logger;
            _matches = new();
            _teamMatches = new();
        }

        internal void Clear()
        {
            _teamMatches.Clear();
            _matches.Clear();
        }
        internal IEnumerable<BasicMatch> GetMatches()
        {
            return _matches;
        }

        internal IEnumerable<BasicMatch> GetMatches(Team team)
        {
            return _teamMatches.GetValueOrDefault(team) ?? Enumerable.Empty<BasicMatch>();
        }

        internal bool Contains(BasicMatch match)
        {
            return _matches.Contains(match);
        }

        internal void Add(BasicMatch match)
        {
            _logger.LogDebug($"MatchStore Add({match})");
            _matches.Add(match);
            AddMatch(match.Team1, match);
            AddMatch(match.Team2, match);
        }

        internal bool Remove(BasicMatch match)
        {
            _logger.LogDebug($"MatchStore Remove({match})");
            RemoveMatch(match.Team1, match);
            RemoveMatch(match.Team2, match);
            return _matches.TryRemove(match);
        }

        private bool RemoveMatch(Team team, BasicMatch match)
        {
            if (_teamMatches.ContainsKey(team))
            {
                return _teamMatches[team].TryRemove(match);
            }
            return false;
        }

        private void AddMatch(Team team, BasicMatch match)
        {
            if (!_teamMatches.ContainsKey(team))
            {
                _teamMatches.TryAdd(team, new());
            }
            _teamMatches[team].Add(match);
        }

        internal void Remove(Team team)
        {
            if (_teamMatches.ContainsKey(team))
            {
                var matches = _teamMatches[team].ToList();
                foreach (var match in matches)
                {
                    if (!match.MatchState.TriggerLaunchGame)
                    {
                        Remove(match);
                    }
                }
            }
        }
    }
}