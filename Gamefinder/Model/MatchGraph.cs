
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

        internal bool IsDialogActive(Match match) => _dialogManager.IsDialogActive(match);

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
                        if (_eventQueue.TryTake(out Action? action, TimeSpan.FromSeconds(1)))
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
                match.Tick();
            }

            foreach (var coach in _coaches)
            {
                if (coach.IsTimedOut)
                {
                    Console.WriteLine($"{coach} timed out");
                    RemoveCoach(coach);
                }
            }
        }

        public void Dispatch(Action action) => _eventQueue.Add(action);

        public void AddTeam(Team team) => _eventQueue.Add(() => InternalAddTeam(team));
        public void RemoveTeam(Team team) => _eventQueue.Add(() => InternalRemoveTeam(team));
        public void AddCoach(Coach coach) => _eventQueue.Add(() => InternalAddCoach(coach));
        public void RemoveCoach(Coach coach) => _eventQueue.Add(() => InternalRemoveCoach(coach));
        public void AddTeamToCoach(Team team, Coach coach) => _eventQueue.Add(() => InternalAddTeamToCoach(team, coach));
        public async Task<List<Match>> GetMatchesAsync(Coach coach) => await Serialized<Coach, List<Match>>(InternalGetMatches, coach);
        public async Task<List<Match>> GetMatches() => await Serialized<List<Match>>(InternalGetMatches);
        public async Task<List<Team>> GetTeams() => await Serialized<List<Team>>(InternalGetTeams);
        public async Task<Match?> GetMatch(Team team1, Team team2) => await Serialized<Team, Team, Match?>(InternalGetMatch, team1, team2);
        public void TriggerLaunchGame(Match match) => _eventQueue.Add(() => InternalTriggerLaunchGame(match));
        public void TriggerStartDialog(Match match) => _eventQueue.Add(() => InternalTriggerStartDialog(match));
        public void ClearDialog(Match match) => _eventQueue.Add(() => InternalClearDialog(match));

        internal void InternalClearDialog(Match match)
        {
            _dialogManager.Remove(match);
        }

        private void InternalTriggerStartDialog(Match match)
        {
            _dialogManager.Add(match);
        }

        private void InternalTriggerLaunchGame(Match match)
        {
            var coach1 = match.Team1.Coach;
            var coach2 = match.Team2.Coach;

            _dialogManager.Remove(match);

            coach1.Lock();
            coach2.Lock();

            foreach (var team in coach1.GetTeams().Concat(coach2.GetTeams()))
            {
                foreach (var m in team.GetMatches())
                {
                    if (!m.Equals(match))
                    {
                        m.Act(TeamAction.Cancel);
                    }
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
                    var m = new Match(this, opponent, team);
                    Console.WriteLine($"Adding {m}");
                    _matches.Add(m);
                    opponent.Add(m);
                    team.Add(m);
                    MatchAdded?.Invoke(this, new MatchUpdatedArgs { Match = m });
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
                    MatchRemoved?.Invoke(this, new MatchUpdatedArgs { Match = match });

                }
            }
            _teams.TryRemove(team);
            TeamRemoved?.Invoke(this, new TeamUpdatedArgs { Team = team });
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
            _eventQueue.Add(() => func(result));
            return result.Task;
        }

        private Task<T> Serialized<P, T>(Action<P, TaskCompletionSource<T>> func, P param)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _eventQueue.Add(() => func(param, result));
            return result.Task;
        }

        private Task<T> Serialized<P1, P2, T>(Action<P1, P2, TaskCompletionSource<T>> func, P1 param1, P2 param2)
        {
            TaskCompletionSource<T> result = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _eventQueue.Add(() => func(param1, param2, result));
            return result.Task;
        }
        #endregion
    }
}
