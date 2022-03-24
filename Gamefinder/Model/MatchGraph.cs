
using ConcurrentCollections;
using Fumbbl.Gamefinder.Model.Event;
using Fumbbl.Gamefinder.Model.Store;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class MatchGraph
    {
        private readonly BlockingCollection<Action> _eventQueue;
        private readonly TeamStore _teams;
        private readonly CoachStore _coaches;
        private readonly MatchStore _matches;
        private readonly DialogManager _dialogManager;

        public event EventHandler? CoachAdded;
        public event EventHandler? CoachRemoved;
        public event EventHandler? TeamAdded;
        public event EventHandler? TeamRemoved;
        public event EventHandler? MatchAdded;
        public event EventHandler? MatchRemoved;
        public event EventHandler? GraphUpdated;
        public event EventHandler? MatchLaunched;

        public DialogManager DialogManager => _dialogManager;
        public bool IsDialogActive(Match match) => _dialogManager.IsDialogActive(match);

        private readonly TimeSpan TickTimeout = TimeSpan.FromSeconds(1);

        public MatchGraph()
        {
            _teams = new();
            _coaches = new();
            _matches = new();
            _eventQueue = new();
            _dialogManager = new();
        }

        public void Start()
        {
            Task.Run((Action)(() =>
            {
                var lastTick = DateTime.Now;
                while (!_eventQueue.IsAddingCompleted)
                {
                    try
                    {
                        if (_eventQueue.TryTake(out Action? action, TickTimeout))
                        {
                            action.Invoke();
                        }
                        if ((DateTime.Now - lastTick).TotalMilliseconds > 1000)
                        {
                            Tick();
                            lastTick = DateTime.Now;
                            GraphUpdated?.Invoke((object)this, EventArgs.Empty);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }));
        }

        public void Stop()
        {
            _eventQueue.CompleteAdding();
        }

        private void Tick()
        {
            foreach (var match in _matches.GetMatches())
            {
                (match as Match)?.Tick();
            }

            foreach (var coach in _coaches.GetCoaches())
            {
                if (coach.IsTimedOut)
                {
                    _ = RemoveAsync(coach);
                }
            }
        }

        private void Dispatch(Action action) => _eventQueue.Add(action);

        public Task DispatchAsync(Action action)
        {
            action.Invoke();
            return Task.CompletedTask;
        }

        public async Task DispatchAsync(Func<Task> asyncAction)
        {
            TaskCompletionSource result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(async () =>
                {
                    await asyncAction.Invoke();
                    result.SetResult();
                }
            );
            await result.Task;
        }

        public async Task AddAsync(Team team) => await DispatchAsync(() => InternalAddTeam(team));
        public async Task RemoveAsync(Team team) => await DispatchAsync(() => InternalRemoveTeam(team));
        public async Task AddAsync(Coach coach) => await DispatchAsync(() => InternalAddCoach(coach));
        public async Task RemoveAsync(Coach coach) => await DispatchAsync(() => InternalRemoveCoach(coach));
        public async Task RemoveAsync(BasicMatch match) => await DispatchAsync(() => InternalRemoveMatch(match));
        public async Task<List<BasicMatch>> GetMatchesAsync(Coach coach) => await Serialized<Coach, List<BasicMatch>>(InternalGetMatches, coach);
        public async Task<List<BasicMatch>> GetMatchesAsync() => await Serialized<List<BasicMatch>>(InternalGetMatches);
        public async Task<List<Coach>> GetCoachesAsync() => await Serialized<List<Coach>>(InternalGetCoaches);
        public async Task<List<Team>> GetTeamsAsync() => await Serialized<List<Team>>(InternalGetTeams);
        public async Task<List<Team>> GetTeamsAsync(Coach coach) => await Serialized<Coach, List<Team>>(InternalGetTeams, coach);
        public async Task<BasicMatch?> GetMatchAsync(Team team1, Team team2) => await Serialized<Team, Team, BasicMatch?>(InternalGetMatch, team1, team2);
        public async Task TriggerLaunchGameAsync(BasicMatch match) => await DispatchAsync(() => InternalTriggerLaunchGame(match));
        public async Task TriggerStartDialogAsync(Match match) => await DispatchAsync(() => InternalTriggerStartDialog(match));
        public async Task ClearDialogAsync(Match match) => await DispatchAsync(() => InternalClearDialog(match));
        public async Task<BasicMatch?> GetStartDialogMatch(Coach coach) => await Serialized<Coach, BasicMatch?>(InternalGetStartDialogMatch, coach);

        private void InternalGetStartDialogMatch(Coach coach, TaskCompletionSource<BasicMatch?> result)
        {
            result.SetResult(_dialogManager.GetActiveDialog(coach));
        }

        internal void InternalClearDialog(Match match)
        {
            _dialogManager.Remove(match);
        }

        private void InternalTriggerStartDialog(Match match)
        {
            _dialogManager.Add(match);
        }

        private async Task InternalTriggerLaunchGame(BasicMatch match)
        {
            var coach1 = match.Team1.Coach;
            var coach2 = match.Team2.Coach;

            _dialogManager.Remove(match);

            coach1.Lock();
            coach2.Lock();

            foreach (var m in _teams.GetTeams(coach1).Concat(_teams.GetTeams(coach2)).SelectMany(t => _matches.GetMatches(t)))
            {
                if (!m.Equals(match) && m is Match m1)
                {
                    await m1.ActAsync(TeamAction.Cancel);
                }
            }
            MatchLaunched?.Invoke(this, new MatchUpdatedArgs(match));
        }

        private void InternalGetCoaches(TaskCompletionSource<List<Coach>> result)
            => result.SetResult(new List<Coach>(_coaches.GetCoaches()));

        private void InternalGetTeams(TaskCompletionSource<List<Team>> result)
            => result.SetResult(new List<Team>(_teams.GetTeams()));

        private void InternalGetTeams(Coach coach, TaskCompletionSource<List<Team>> result)
            => result.SetResult(new List<Team>(_teams.GetTeams(coach)));

        private void InternalGetMatches(TaskCompletionSource<List<BasicMatch>> result)
            => result.SetResult(new List<BasicMatch>(_matches.GetMatches()));

        private void InternalGetMatches(Coach coach, TaskCompletionSource<List<BasicMatch>> result)
            => result.SetResult(new List<BasicMatch>(_teams.GetTeams(coach).SelectMany(t => _matches.GetMatches(t))));

        public void InternalGetMatch(Team team1, Team team2, TaskCompletionSource<BasicMatch?> result)
            => result.SetResult(_matches.GetMatches(team1).Where(m => m.Includes(team2)).FirstOrDefault());

        private void InternalAddTeam(Team team)
        {
            if (team is null || _teams.Contains(team))
            {
                return;
            }

            if (!_coaches.Contains(team.Coach))
            {
                _coaches.Add(team.Coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = team.Coach });
            }

            _teams.Add(team);
            TeamAdded?.Invoke(this, new TeamUpdatedArgs { Team = team });
            foreach (var opponent in _teams.GetTeams())
            {
                if (team is not null && team.IsOpponentAllowed(opponent) && !opponent.Coach.Locked)
                {
                    var match = new Match(this, opponent, team);
                    _matches.Add(match);
                    MatchAdded?.Invoke(this, new MatchUpdatedArgs(match));
                }
            }
        }

        private void InternalRemoveTeam(Team team)
        {
            if (team is null || !_teams.Contains(team))
            {
                return;
            }
            _dialogManager.Remove(team);

            foreach (var match in _matches.GetMatches(team))
            {
                var t = match.GetOpponent(team);
                if (t is not null)
                {
                    _matches.Remove(match);
                    if (match.MatchState.TriggerLaunchGame)
                    {
                        match.Team1.Coach.Unlock();
                        match.Team2.Coach.Unlock();
                    }

                    _dialogManager.Remove(match);
                    MatchRemoved?.Invoke(this, new MatchUpdatedArgs(match));

                }
            }
            _matches.Remove(team);
            _teams.Remove(team);
            TeamRemoved?.Invoke(this, new TeamUpdatedArgs { Team = team });
        }

        private void InternalRemoveMatch(BasicMatch match)
        {
            if (match is null || !_matches.Contains(match))
            {
                return;
            }
            _dialogManager.Remove(match);

            _matches.Remove(match);
            if (match.MatchState.TriggerLaunchGame)
            {
                match.Team1.Coach.Unlock();
                match.Team2.Coach.Unlock();
            }

            MatchRemoved?.Invoke(this, new MatchUpdatedArgs(match));
        }

        private void InternalAddCoach(Coach coach)
        {
            if (!_coaches.Contains(coach))
            {
                _coaches.Add(coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
            }
        }

        private void InternalRemoveCoach(Coach coach)
        {
            _dialogManager.Remove(coach);

            foreach (var team in _teams.GetTeams(coach))
            {
                InternalRemoveTeam(team);
            }
            _coaches.Remove(coach);
            CoachRemoved?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
        }

        #region Serialized() helper methods

        private Task<T> Serialized<T>(Action<TaskCompletionSource<T>> func)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(result));
            return result.Task;
        }

        private Task<T> Serialized<P, T>(Action<P, TaskCompletionSource<T>> func, P param)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(param, result));
            return result.Task;
        }

        private Task<T> Serialized<P1, P2, T>(Action<P1, P2, TaskCompletionSource<T>> func, P1 param1, P2 param2)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => func(param1, param2, result));
            return result.Task;
        }
        #endregion
    }
}
