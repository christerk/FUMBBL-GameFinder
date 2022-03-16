
using ConcurrentCollections;
using Fumbbl.Gamefinder.Model.Event;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class MatchGraph
    {
        private readonly GamefinderModel _gameFinder;
        private readonly BlockingCollection<Action> _eventQueue;
        private readonly ConcurrentHashSet<Team> _teams;
        private readonly ConcurrentHashSet<Coach> _coaches;
        private readonly ConcurrentHashSet<Match> _matches;
        private readonly DialogManager _dialogManager;

        public event EventHandler? CoachAdded;
        public event EventHandler? CoachRemoved;
        public event EventHandler? TeamAdded;
        public event EventHandler? TeamRemoved;
        public event EventHandler? MatchAdded;
        public event EventHandler? MatchRemoved;
        public event EventHandler? GraphUpdated;
        public event EventHandler? QueueIsEmpty;

        public DialogManager DialogManager => _dialogManager;
        public bool IsDialogActive(Match match) => _dialogManager.IsDialogActive(match);

        private TimeSpan TickTimeout = TimeSpan.FromSeconds(1);

        public MatchGraph(GamefinderModel gameFinder)
        {
            _gameFinder = gameFinder;
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
                        else
                        {
                            QueueIsEmpty?.Invoke(this, EventArgs.Empty);
                        }
                        if ((DateTime.Now - lastTick).TotalMilliseconds > 1000)
                        {
                            Tick();
                            lastTick = DateTime.Now;
                            GraphUpdated?.Invoke((object)this, EventArgs.Empty);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                Console.WriteLine($"MatchGraph Task Ending");
            }));
        }

        public void Stop()
        {
            Console.WriteLine($"Halting MatchGraph Task");
            _eventQueue.CompleteAdding();
        }

        private void Tick()
        {
            foreach (var match in _matches)
            {
                match.Tick();
            }

            foreach (var coach in _coaches)
            {
                if (coach.IsTimedOut)
                {
                    Console.WriteLine($"{coach} timed out");
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
        public async Task RemoveAsync(Match match) => await DispatchAsync(() => InternalRemoveMatch(match));
        public async Task AddTeamToCoachAsync(Team team, Coach coach) => await DispatchAsync(() => InternalAddTeamToCoach(team, coach));
        public async Task<List<Match>> GetMatchesAsync(Coach coach) => await Serialized<Coach, List<Match>>(InternalGetMatches, coach);
        public async Task<List<Match>> GetMatchesAsync() => await Serialized<List<Match>>(InternalGetMatches);
        public async Task<List<Team>> GetTeamsAsync() => await Serialized<List<Team>>(InternalGetTeams);
        public async Task<Match?> GetMatchAsync(Team team1, Team team2) => await Serialized<Team, Team, Match?>(InternalGetMatch, team1, team2);
        public async Task TriggerLaunchGameAsync(Match match) => await DispatchAsync(() => InternalTriggerLaunchGame(match));
        public async Task TriggerStartDialogAsync(Match match) => await DispatchAsync(() => InternalTriggerStartDialog(match));
        public async Task ClearDialogAsync(Match match) => await DispatchAsync(() => InternalClearDialog(match));
        
        public async Task WaitForEmptyQueueAsync()
        {
            TaskCompletionSource taskCompletionSource = new();

            Action complete = () => taskCompletionSource.SetResult();

            QueueIsEmpty += (object? src, EventArgs args) => complete.Invoke();

            await taskCompletionSource.Task;
        }

        internal void InternalClearDialog(Match match)
        {
            _dialogManager.Remove(match);
        }

        private void InternalTriggerStartDialog(Match match)
        {
            _dialogManager.Add(match);
        }

        private async Task InternalTriggerLaunchGame(Match match)
        {
            var coach1 = match.Team1.Coach;
            var coach2 = match.Team2.Coach;

            _dialogManager.Remove(match);

            coach1.Lock();
            coach2.Lock();

            foreach (var m in coach1.GetTeams().Concat(coach2.GetTeams()).SelectMany(t => t.GetMatches()))
            {
                if (!m.Equals(match))
                {
                    await m.ActAsync(TeamAction.Cancel);
                }
            }
        }

        private void InternalGetTeams(TaskCompletionSource<List<Team>> result)
            => result.SetResult(new List<Team>(_teams));

        private void InternalGetMatches(TaskCompletionSource<List<Match>> result)
            => result.SetResult(new List<Match>(_matches));

        private void InternalGetMatches(Coach coach, TaskCompletionSource<List<Match>> result)
            => result.SetResult(new List<Match>(coach.GetTeams().SelectMany(t => t.GetMatches())));

        public void InternalGetMatch(Team team1, Team team2, TaskCompletionSource<Match?> result)
            => result.SetResult(team1.GetMatches().Where(m => m.Includes(team2)).FirstOrDefault());

        private void InternalAddTeam(Team team)
        {
            if (team is null || _teams.Contains(team))
            {
                return;
            }

            if (!_coaches.Contains(team.Coach))
            {
                Console.WriteLine($"Adding {team.Coach}");
                _coaches.Add(team.Coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = team.Coach });
            }

            Console.WriteLine($"Adding {team}");
            _teams.Add(team);
            TeamAdded?.Invoke(this, new TeamUpdatedArgs { Team = team });
            foreach (var opponent in _teams)
            {
                if (team is not null && team.IsOpponentAllowed(opponent) && !opponent.Coach.Locked)
                {
                    var match = new Match(this, opponent, team);
                    Console.WriteLine($"Adding {match}");
                    _matches.Add(match);
                    opponent.Add(match);
                    team.Add(match);
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

            foreach (var match in team.GetMatches())
            {
                var t = match.GetOpponent(team);
                if (t is not null)
                {
                    Console.WriteLine($"Removing {match}");
                    t.Remove(match);
                    _matches.TryRemove(match);
                    if (match.MatchState.TriggerLaunchGame)
                    {
                        match.Team1.Coach.Unlock();
                        match.Team2.Coach.Unlock();
                    }

                    _dialogManager.Remove(match);
                    MatchRemoved?.Invoke(this, new MatchUpdatedArgs(match));

                }
            }
            _teams.TryRemove(team);
            TeamRemoved?.Invoke(this, new TeamUpdatedArgs { Team = team });
        }

        private void InternalRemoveMatch(Match match)
        {
            if (match is null || !_matches.Contains(match))
            {
                return;
            }
            _dialogManager.Remove(match);

            Console.WriteLine($"Removing {match}");
            if (_matches.TryRemove(match))
            {
                match.Team1.Remove(match);
                match.Team2.Remove(match);
            }
            if (match.MatchState.TriggerLaunchGame)
            {
                match.Team1.Coach.Unlock();
                match.Team2.Coach.Unlock();
            }

            MatchRemoved?.Invoke(this, new MatchUpdatedArgs(match));
        }

        private void InternalAddCoach(Coach coach)
        {
            Console.WriteLine($"Adding {coach}");
            if (!_coaches.Contains(coach))
            {
                _coaches.Add(coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
            }

            foreach (var team in coach.GetTeams())
            {
                InternalAddTeam(team);
            }
        }

        private void InternalRemoveCoach(Coach coach)
        {
            Console.WriteLine($"Removing {coach}");

            _dialogManager.Remove(coach);

            foreach (var team in coach.GetTeams())
            {
                InternalRemoveTeam(team);
            }
            _coaches.TryRemove(coach);
            CoachRemoved?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
        }

        private static void InternalAddTeamToCoach(Team team, Coach coach)
        {
            coach.Add(team);
        }

        #region Serialized() helper methods
        private Task Serialized()
        {
            TaskCompletionSource result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Dispatch(() => result.SetResult());
            return result.Task;
        }

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
