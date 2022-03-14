using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    internal class DialogManager
    {
        private readonly ConcurrentDictionary<Match, StartDialog> _startDialogs;
        private readonly HashSet<Coach> _activeCoaches;

        public DialogManager()
        {
            _startDialogs = new();
            _activeCoaches = new();
        }

        internal bool IsDialogActive(Match match)
        {
            if (_startDialogs.TryGetValue(match, out var dialog))
            {
                return dialog.Active;
            }
            return false;
        }

        internal bool HasDialog(Coach c1, Coach c2)
        {
            return _startDialogs.Values.Any(d => d.Coach1 == c1 || d.Coach1 == c2 || d.Coach2 == c1 || d.Coach2 == c2);
        }

        internal void Add(Match match)
        {
            _startDialogs.TryAdd(match, new StartDialog(match));
            Rescan();
        }

        public void Rescan()
        {
            foreach (var d in _startDialogs.Values)
            {
                if (!d.Active && !_activeCoaches.Contains(d.Coach1) && !_activeCoaches.Contains(d.Coach2))
                {
                    Verify(d.Coach1, d.Coach2);
                    d.Active = true;
                    _activeCoaches.Add(d.Coach1);
                    _activeCoaches.Add(d.Coach2);
                }
            }
        }

        private void Verify(Coach c1, Coach c2)
        {
            var error = _startDialogs.Values.Any(
                d =>
                    (d.Coach1.Equals(c1) || d.Coach2.Equals(c1) || d.Coach1.Equals(c2) || d.Coach2.Equals(c2))
                    &&
                    d.Active
            );

            if (error)
            {

            }
        }

        internal void Remove(Match match, bool rescan = true)
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

        internal void Remove(Team team)
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

        internal void Remove(Coach coach)
        {
            var dialogs = _startDialogs.Where(p => (p.Value.Coach1?.Equals(coach) ?? false) || (p.Value.Coach2?.Equals(coach) ?? false));
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

        private static void Unlock(Match match)
        {
            if (match.MatchState.TriggerLaunchGame)
            {
                match.Team1.Coach.Unlock();
                match.Team2.Coach.Unlock();
            }
        }
    }
}