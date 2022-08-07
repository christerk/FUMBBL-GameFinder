
using ConcurrentCollections;
using Fumbbl.Gamefinder.Model.Event;
using Fumbbl.Gamefinder.Model.Store;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class MatchGraph
    {
        private EventQueue _eventQueue;
        private bool _tickEnabled = true;
        private ISchedulingContext _schedulingContext;
        private readonly TeamStore _teams;
        private readonly CoachStore _coaches;
        private readonly MatchStore _matches;
        private readonly DialogManager _dialogManager;
        internal readonly ILogger<MatchGraph> Logger;

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

        public MatchGraph(ILoggerFactory loggerFactory, EventQueue eventQueue, ISchedulingContext context)
        {
            Logger = loggerFactory.CreateLogger<MatchGraph>();
            _teams = new(Logger);
            _coaches = new(Logger);
            _matches = new(Logger);
            _dialogManager = new(loggerFactory);
            _eventQueue = eventQueue;
            _eventQueue.Tick += HandleTick;
            _schedulingContext = context;
        }

        public void DisableTick()
        {
            _tickEnabled = false;
        }

        private void HandleTick(object? sender, EventArgs e)
        {
            if (_tickEnabled)
            {
                Tick();
                GraphUpdated?.Invoke((object)this, EventArgs.Empty);
            }
        }

        private void Tick()
        {
            foreach (var match in _matches.GetMatches())
            {
                (match as Match)?.Tick();
            }

            foreach (var coach in _coaches.GetCoaches())
            {
                if (_coaches.IsTimedOut(coach))
                {
                    Logger.LogDebug($"Timed out {coach}");
                    Remove(coach);
                }
            }
        }

        internal void SetGameId(BasicMatch match, int clientId)
        {
            match.ClientId = clientId;
        }

        public void Ping(Coach coach, int offsetSeconds = 0)
        {
            _coaches.Ping(coach, offsetSeconds);
        }

        internal void SetSchedulingError(BasicMatch match, string errorMessage)
        {
            match.SchedulingError = errorMessage;
            Logger.LogError($"Error scueduling {match}: {errorMessage}");
        }

        public void Reset()
        {
            Logger.LogDebug("Resetting Graph");
            _dialogManager.Clear();
            _matches.Clear();
            _teams.Clear();
            _coaches.Clear();
        }

        internal void InjectLaunchedMatch(BasicMatch match)
        {
            var injectedMatch = new Match(this, match.Team1, match.Team2);
            injectedMatch.ForceLaunch();
            injectedMatch.ClientId = -1;
            _matches.Add(injectedMatch);
            Logger.LogDebug($"Injecting {injectedMatch} with timeout set to {injectedMatch.TimeUntilReset}ms");
        }

        private void GetStartDialogMatch(Coach coach, TaskCompletionSource<BasicMatch?> result)
        {
            result.SetResult(_dialogManager.GetActiveDialog(coach));
        }

        internal void ClearDialog(Match match)
        {
            Logger.LogDebug($"Clearing StartDialog for match {match}");
            _dialogManager.Remove(match);
        }

        public void TriggerStartDialog(Match match)
        {
            Logger.LogDebug($"Adding StartDialog for match {match}");
            _dialogManager.Add(match);
        }

        public void TriggerLaunchGame(BasicMatch match)
        {
            Logger.LogDebug($"Launching Match {match}");
            var coach1 = match.Team1.Coach;
            var coach2 = match.Team2.Coach;

            _dialogManager.Remove(match);
            _dialogManager.Remove(coach1);
            _dialogManager.Remove(coach2);

            coach1.Lock();
            coach2.Lock();

            foreach (var m in _teams.GetTeams(coach1).Concat(_teams.GetTeams(coach2)).SelectMany(t => _matches.GetMatches(t)))
            {
                if (!m.Equals(match) && m is Match m1)
                {
                    m1.Act(TeamAction.Cancel);
                }
            }
            MatchLaunched?.Invoke(this, new MatchUpdatedArgs(match));
        }

        public IEnumerable<Coach> GetCoaches()
            => new List<Coach>(_coaches.GetCoaches());

        public IEnumerable<Team> GetTeams()
            => new List<Team>(_teams.GetTeams());

        public IEnumerable<Team> GetTeams(Coach coach)
            => _teams.GetTeams(coach);

        public IEnumerable<BasicMatch> GetMatches()
            => new List<BasicMatch>(_matches.GetMatches());

        public IEnumerable<BasicMatch> GetMatches(Coach coach)
            => new List<BasicMatch>(_teams.GetTeams(coach).SelectMany(t => _matches.GetMatches(t)));

        public BasicMatch? GetMatch(Team team1, Team team2)
            => _matches.GetMatches(team1).Where(m => m.Includes(team2)).FirstOrDefault();

        public bool Contains(Coach coach)
            => _coaches.Contains(coach);


        public void Add(Team team)
        {
            Logger.LogTrace($"Adding team {team} {team.TvLimit} Ruleset({team.RulesetId})");
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
                if (team is not null && _schedulingContext.IsOpponentAllowed(team, opponent) && !opponent.Coach.Locked)
                {
                    var match = new Match(this, opponent, team);
                    _matches.Add(match);
                    MatchAdded?.Invoke(this, new MatchUpdatedArgs(match));
                }
            }
        }

        public void Remove(Team team)
        {
            Logger.LogTrace($"Removing team {team}");
            if (team is null || !_teams.Contains(team))
            {
                return;
            }
            _dialogManager.Remove(team);

            foreach (var match in _matches.GetMatches(team))
            {
                if (match.MatchState.TriggerLaunchGame)
                {
                    // Don't remove games marked for launching, they will be removed separately.
                    continue;
                }
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

        public void Remove(BasicMatch match)
        {
            Logger.LogTrace($"Removing Match {match}");
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

        public void Add(Coach coach)
        {
            Logger.LogDebug($"Adding coach {coach}");
            if (!_coaches.Contains(coach))
            {
                _coaches.Add(coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
            }
        }

        public void Remove(Coach coach)
        {
            Logger.LogDebug($"Removing coach {coach}");
            _dialogManager.Remove(coach);

            foreach (var team in _teams.GetTeams(coach))
            {
                Remove(team);
            }
            _coaches.Remove(coach);
            CoachRemoved?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
        }
    }
}
