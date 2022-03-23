﻿
using ConcurrentCollections;
using Fumbbl.Gamefinder.Model.Event;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class MatchGraph
    {
        private readonly BlockingCollection<Action> _eventQueue;
        private readonly ConcurrentHashSet<Team> _teams;
        private readonly ConcurrentHashSet<Coach> _coaches;
        private readonly ConcurrentHashSet<BasicMatch> _matches;
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

        private TimeSpan TickTimeout = TimeSpan.FromSeconds(1);

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
                (match as Match)?.Tick();
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
        public async Task RemoveAsync(BasicMatch match) => await DispatchAsync(() => InternalRemoveMatch(match));
        public async Task AddTeamToCoachAsync(Team team, Coach coach) => await DispatchAsync(() => InternalAddTeamToCoach(team, coach));
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

            foreach (var m in coach1.GetTeams().Concat(coach2.GetTeams()).SelectMany(t => t.GetMatches()))
            {
                if (!m.Equals(match) && m is Match m1)
                {
                    await m1.ActAsync(TeamAction.Cancel);
                }
            }
            MatchLaunched?.Invoke(this, new MatchUpdatedArgs(match));
        }

        private void InternalGetCoaches(TaskCompletionSource<List<Coach>> result)
            => result.SetResult(new List<Coach>(_coaches));

        private void InternalGetTeams(TaskCompletionSource<List<Team>> result)
            => result.SetResult(new List<Team>(_teams));

        private void InternalGetTeams(Coach coach, TaskCompletionSource<List<Team>> result)
            => result.SetResult(new List<Team>(coach.GetTeams()));

        private void InternalGetMatches(TaskCompletionSource<List<BasicMatch>> result)
            => result.SetResult(new List<BasicMatch>(_matches));

        private void InternalGetMatches(Coach coach, TaskCompletionSource<List<BasicMatch>> result)
            => result.SetResult(new List<BasicMatch>(coach.GetTeams().SelectMany(t => t.GetMatches())));

        public void InternalGetMatch(Team team1, Team team2, TaskCompletionSource<BasicMatch?> result)
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

        private void InternalRemoveMatch(BasicMatch match)
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
