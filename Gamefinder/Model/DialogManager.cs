using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    internal class DialogManager
    {
        private readonly ConcurrentDictionary<Match, StartDialog> _startDialogs;

        public DialogManager()
        {
            _startDialogs = new();
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

        internal void Add(Match match, Coach c1, Coach c2, bool active)
        {
            _startDialogs.TryAdd(match, new StartDialog() { Match = match, Coach1 = c1, Coach2 = c2, Active = active });
        }

        public void Rescan()
        {
            HashSet<Coach> coaches = new();
            foreach (var pair in _startDialogs)
            {
                var match = pair.Key;
                var d = pair.Value;
                if (d.Coach1 is not null && d.Coach2 is not null)
                {
                    if (!d.Active && !coaches.Contains(d.Coach1) && !coaches.Contains(d.Coach2))
                    {
                        d.Active = true;
                    }
                    if (d.Active)
                    {
                        coaches.Add(d.Coach1);
                        coaches.Add(d.Coach2);
                    }
                }
            }
        }

        internal void Remove(Match match)
        {
            if (_startDialogs.TryRemove(match, out _))
            {
                Rescan();
            }
        }

        internal void Remove(Team team)
        {
            var dialogs = _startDialogs.Where(p => p.Key.Team1.Equals(team) || p.Key.Team2.Equals(team));
            if (dialogs.Count() > 0)
            {
                foreach (var dialog in dialogs)
                {
                    _startDialogs.Remove(dialog.Key, out _);
                }
                Rescan();
            }
        }

        internal void Remove(Coach coach)
        {
            var dialogs = _startDialogs.Where(p => (p.Value.Coach1?.Equals(coach) ?? false) || (p.Value.Coach2?.Equals(coach) ?? false));
            if (dialogs.Count() > 0)
            {
                foreach (var dialog in dialogs)
                {
                    Unlock(dialog.Key);
                    _startDialogs.Remove(dialog.Key, out _);
                }
                Rescan();
            }
        }

        private void Unlock(Match match)
        {
            if (match.MatchState.TriggerLaunchGame)
            {
                match.Team1.Coach.Unlock();
                match.Team2.Coach.Unlock();
            }
        }
    }
}