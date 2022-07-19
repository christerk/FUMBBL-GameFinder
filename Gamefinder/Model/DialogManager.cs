using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class DialogManager
    {
        private readonly ILogger<DialogManager> _logger;
        private readonly ConcurrentDictionary<BasicMatch, StartDialog> _startDialogs;
        private readonly HashSet<Coach> _activeCoaches;

        public DialogManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DialogManager>();
            _startDialogs = new();
            _activeCoaches = new();
        }

        public bool IsDialogActive(BasicMatch match)
        {
            if (_startDialogs.TryGetValue(match, out var dialog))
            {
                return dialog.Active;
            }
            return false;
        }

        public void Add(BasicMatch match)
        {
            _startDialogs.TryAdd(match, new StartDialog(match));
            Rescan();
        }

        public List<StartDialog> GetDialogs()
        {
            return _startDialogs.Values.ToList();
        }

        public void Clear()
        {
            _startDialogs?.Clear();
            _activeCoaches?.Clear();
        }

        public bool Contains(BasicMatch match)
        {
            return _startDialogs.ContainsKey(match);
        }

        public void Rescan()
        {
            foreach (var d in _startDialogs.Values)
            {
                if (!d.Active && !_activeCoaches.Contains(d.Coach1) && !_activeCoaches.Contains(d.Coach2))
                {
                    d.Active = true;
                    _activeCoaches.Add(d.Coach1);
                    _activeCoaches.Add(d.Coach2);
                }
            }
        }

        public void Remove(BasicMatch match, bool rescan = true)
        {
            if (_startDialogs.TryRemove(match, out var dialog))
            {
                if (dialog.Active)
                {
                    _activeCoaches.Remove(match.Team1.Coach);
                    _activeCoaches.Remove(match.Team2.Coach);
                }
                if (rescan)
                {
                    Rescan();
                }
            }
        }

        public void Remove(Team team)
        {
            var dialogs = _startDialogs.Where(p => p.Key.Team1.Equals(team) || p.Key.Team2.Equals(team));
            if (dialogs.Any())
            {
                foreach (var dialog in dialogs)
                {
                    if (_startDialogs.TryRemove(dialog.Key, out _))
                    {
                        Remove(dialog.Key, false);
                    }
                }
                Rescan();
            }
        }

        public void Remove(Coach coach)
        {
            var dialogs = _startDialogs.Where(p => p.Value.Coach1.Equals(coach) || p.Value.Coach2.Equals(coach));
            if (dialogs.Any())
            {
                foreach (var dialog in dialogs)
                {
                    Unlock(dialog.Key);
                    Remove(dialog.Key, false);
                }
                Rescan();
            }
        }

        private static void Unlock(BasicMatch match)
        {
            if (match.MatchState.TriggerLaunchGame)
            {
                match.Team1.Coach.Unlock();
                match.Team2.Coach.Unlock();
            }
        }

        public BasicMatch? GetActiveDialog(Coach coach)
        {
            return _startDialogs.FirstOrDefault(p => p.Value.Active && (p.Value.Coach1.Equals(coach) || p.Value.Coach2.Equals(coach))).Key;
        }
    }
}