using Fumbbl.Api;
using Fumbbl.Gamefinder.Model.Event;

namespace Fumbbl.Gamefinder.Model
{
    public class GamefinderModel
    {
        private readonly MatchGraph _matchGraph;
        private EventQueue _eventQueue;
        private ILogger<GamefinderModel> _logger;
        private FumbblApi _fumbbl;

        public MatchGraph Graph => _matchGraph;

        public GamefinderModel(EventQueue eventQueue, ILoggerFactory loggerFactory, FumbblApi fumbblApi)
        {
            _eventQueue = eventQueue;
            _matchGraph = new MatchGraph(loggerFactory, eventQueue);
            _matchGraph.MatchLaunched += MatchLaunched;
            _logger = loggerFactory.CreateLogger<GamefinderModel>();
            _fumbbl = fumbblApi;
            Start();
        }

        public void Start()
        {
            _eventQueue?.Start();
        }

        public void Stop()
        {
            _eventQueue?.Stop();
        }

        public void DisableEventHandling()
        {
            _matchGraph.MatchLaunched -= MatchLaunched;
        }

        private async void MatchLaunched(object? sender, EventArgs args)
        {
            await _eventQueue.DispatchAsync(async () =>
            {
                // Check that the match is allowed
                if (args is MatchUpdatedArgs e && e?.Match != null)
                {
                    var gameState = await _fumbbl.GameState.CheckAsync(e.Match.Team1.Id, e.Match.Team2.Id);

                    if (gameState is null)
                    {
                        _matchGraph.SetSchedulingError(e.Match, "Unable to verify game validity");
                        return;
                    }

                    if (!string.Equals(gameState.Result, "OK"))
                    {
                        _matchGraph.SetSchedulingError(e.Match, gameState.Message);
                        return;
                    }

                    _logger.LogInformation($"GameState check OK for {e.Match}");

                    _matchGraph.Ping(e.Match.Team1.Coach, Match.LAUNCHED_TIMEOUT);
                    _matchGraph.Ping(e.Match.Team2.Coach, Match.LAUNCHED_TIMEOUT);

                    // Call to FUMBBL API to start the game
                    gameState = await _fumbbl.GameState.ScheduleAsync(e.Match.Team1.Id, e.Match.Team2.Id);
                    if (gameState != null && string.Equals(gameState.Result, "OK"))
                    {
                        // Tell MatchGraph which FFB Game ID needs to be redirected to
                        _matchGraph.SetGameId(e.Match, gameState.GameId);
                    }
                    else
                    {
                        _matchGraph.SetSchedulingError(e.Match, gameState?.Message ?? "Error scheduling match");
                    }

                }
            });
        }

        public async Task<object> GetDebugData()
        {
            return await _eventQueue.Serialized<object>((result) =>
            {
                result.SetResult(new
                {
                    Coaches = Graph.GetCoaches(),
                    Teams = Graph.GetTeams(),
                    Matches = Graph.GetMatches(),
                    StartDialogs = Graph.DialogManager.GetDialogs()
                });
            });
        }
        public async void ActivateAsync(Coach activatingCoach, IEnumerable<Team> activatingTeams)
        {
            await _eventQueue.DispatchAsync(() =>
            {
                var coachExists = _matchGraph.Contains(activatingCoach);
                if (!coachExists)
                {
                    _matchGraph.Add(activatingCoach);
                }
                _matchGraph.Ping(activatingCoach);

                var graphTeams = (_matchGraph.GetTeams(activatingCoach)).ToHashSet();
                foreach (var team in activatingTeams)
                {
                    if (!graphTeams.Contains(team))
                    {
                        _matchGraph.Add(team);
                    }
                    else
                    {
                        var graphTeam = graphTeams.Where(t => t.Equals(team)).First();
                        graphTeam.Update(team);
                    }
                }
                foreach (var team in graphTeams)
                {
                    if (!activatingTeams.Contains(team))
                    {
                        _matchGraph.Remove(team);
                    }
                }
            });
        }

        public async Task<Dictionary<Coach, IEnumerable<Team>>> GetCoachesAndTeams()
        {
            return await _eventQueue.Serialized<Dictionary<Coach, IEnumerable<Team>>>((result) =>
            {
                var dict = new Dictionary<Coach, IEnumerable<Team>>();
                var coaches = _matchGraph.GetCoaches();
                foreach (var coach in coaches)
                {
                    dict.Add(coach, _matchGraph.GetTeams(coach));
                }
                result.SetResult(dict);
            });
        }

        public async Task<IEnumerable<Team>> GetActivatedTeamsAsync(Coach coach)
        {
            return await _eventQueue.Serialized<Coach, List<Team>>((coach, result) =>
            {
                if (coach != null)
                {
                    result.SetResult(new List<Team>(_matchGraph.GetTeams(coach)));
                }
                else
                {
                    result.SetResult(new List<Team>());
                }
            }
            , coach);
        }

        public async Task<Dictionary<BasicMatch, MatchInfo>> GetMatches(Coach coach)
        {
            return await _eventQueue.Serialized<Coach, Dictionary<BasicMatch, MatchInfo>>((coach, result) =>
            {
                _matchGraph.Ping(coach);
                if (coach != null)
                {
                    Dictionary<BasicMatch, MatchInfo> dict = new Dictionary<BasicMatch, MatchInfo>();
                    var dialogMatch = _matchGraph.DialogManager.GetActiveDialog(coach);
                    foreach (var match in _matchGraph.GetMatches(coach))
                    {
                        dict.Add(match, new MatchInfo()
                        {
                            ShowDialog = match.Equals(dialogMatch)
                        });
                    }
                    result.SetResult(dict);
                }
                else
                {
                    result.SetResult(new());
                }
            }
            , coach);
        }

        public async Task MakeOffer(Coach coach, int myTeamId, int opponentTeamId)
        {
            await Act(coach, myTeamId, opponentTeamId, TeamAction.Accept);
        }

        public async Task CancelOffer(Coach coach, int myTeamId, int opponentTeamId)
        {
            await Act(coach, myTeamId, opponentTeamId, TeamAction.Cancel);
        }

        public async Task StartGame(Coach coach, int myTeamId, int opponentTeamId)
        {
            await Act(coach, myTeamId, opponentTeamId, TeamAction.Start);
        }

        private async Task Act(Coach coach, int myTeamId, int opponentTeamId, TeamAction action)
        {
            await _eventQueue.DispatchAsync(() =>
            {
                var match = _matchGraph.GetMatches(coach).SingleOrDefault(m => m.IsBetween(myTeamId, opponentTeamId)) as Match;
                var ownTeam = match?.Team1.Id == myTeamId ? match?.Team1 : match?.Team2;

                if (match != null && ownTeam != null && ownTeam.Coach.Id == coach.Id)
                {
                    match.Act(action, ownTeam);
                }
            });
        }
    }
}
