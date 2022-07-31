using Fumbbl.Api;
using Fumbbl.Api.DTO;
using Fumbbl.Gamefinder.Model.Cache;
using System.Diagnostics;

namespace Fumbbl.Gamefinder.Model
{
    public class BlackboxModel
    {
        public const int ACTIVE_DURATION = 5;
        public const int PAUSE_DURATION = 10;

        private ILogger<BlackboxModel> _logger;
        private FumbblApi _fumbbl;
        private EventQueue _eventQueue;
        private readonly MatchGraph _matchGraph;
        private List<BasicMatch> _matches;
        private Random _random = new Random();

        public MatchGraph Graph => _matchGraph;

        private DateTime _previousDraw = DateTime.MinValue;
        private DateTime _nextDraw = DateTime.MaxValue;
        private DateTime _nextActivation = DateTime.MaxValue;
        private DateTime _currentTime = DateTime.Now;
        private Api.DTO.BlackboxStatus? _status;

        public DTO.BlackboxStatus Status => (_nextDraw - _currentTime).TotalSeconds < ACTIVE_DURATION * 60 ? DTO.BlackboxStatus.Active : DTO.BlackboxStatus.Paused;
        public int SecondsRemaining => (int) Math.Floor(((_nextDraw > _nextActivation ? _nextActivation : _nextDraw) - _currentTime).TotalSeconds);
        public BlackboxStatus BlackboxStatus => _status ?? new();

        public DateTime PreviousDraw => _previousDraw;
        public DateTime NextDraw => _nextDraw;
        public DateTime NextActivation => _nextActivation;

        public BlackboxModel(ILoggerFactory loggerFactory, FumbblApi fumbblApi)
        {
            _logger = loggerFactory.CreateLogger<BlackboxModel>();
            _fumbbl = fumbblApi;
            _eventQueue = new EventQueue(loggerFactory.CreateLogger<EventQueue>());
            _eventQueue.Tick += HandleTick;
            _matchGraph = new(loggerFactory, _eventQueue);
            _matches = new List<BasicMatch>();
            _currentTime = DateTime.Now;
            RefreshTimes();
            _eventQueue.Start();
            _logger.LogInformation("Blackbox Starting");
        }

        private void HandleTick(object? sender, EventArgs e)
        {
            _currentTime = DateTime.Now;
            if (_currentTime >= _nextActivation)
            {
                StartActivation();
                RefreshTimes();
            }
            if (_currentTime >= _nextDraw)
            {
                _ = GenerateRound();
                RefreshTimes();
            }
            _ = _eventQueue.DispatchAsync(async () =>
            {
                _status = await _fumbbl.Blackbox.StatusAsync();
            });
        }

        private void RefreshTimes()
        {
            _eventQueue.DispatchAsync(() =>
            {
                var resolution = ACTIVE_DURATION + PAUSE_DURATION;
                _previousDraw = new DateTime(_currentTime.Year, _currentTime.Month, _currentTime.Day, _currentTime.Hour, (_currentTime.Minute / resolution) * resolution, 0);
                _nextDraw = _previousDraw.AddMinutes(resolution);
                _nextActivation = _previousDraw.AddMinutes(PAUSE_DURATION);
                if (_currentTime >= _nextActivation)
                {
                    _nextActivation = _nextDraw.AddMinutes(PAUSE_DURATION);
                }
            });
        }

        private void StartActivation()
        {
            _eventQueue.DispatchAsync(() =>
            {
                _logger.LogInformation("Starting Activation");
            });
        }

        public async Task<List<BasicMatch>> GenerateRound()
        {
            return await _eventQueue.Serialized<List<BasicMatch>>(async (result) =>
            {
                var roundInfo = new BlackboxSchedulerResult();
                await PopulateMatchGraph(roundInfo);
                var matches = ScheduleMatches(roundInfo);
                await _fumbbl.Blackbox.ReportRoundAsync(roundInfo);
                result.SetResult(matches);
            });
        }

        private async Task PopulateMatchGraph(BlackboxSchedulerResult roundInfo)
        {
            _matchGraph.Reset();
            var status = await _fumbbl.Blackbox.StatusAsync();

            if (status is not null)
            {
                var coachCache = new CoachCache(_fumbbl);
                var teamCache = new TeamCache(_fumbbl);
                foreach (var coachId in status.Coaches)
                {
                    var coach = await coachCache.GetOrCreateAsync(coachId);

                    if (coach is not null)
                    {
                        _matchGraph.Add(coach);
                        var teams = await teamCache.GetLfgTeams(coach);
                        foreach (var team in teams.Where(t => ActivatedForBlackbox(t)))
                        {
                            _matchGraph.Add(team);
                            roundInfo.AddActivatedTeam(coach.Id, new BlackboxTeam(team.Id, team.SchedulingTeamValue));
                        }
                    }
                }
            }
        }

        public List<BasicMatch> ScheduleMatches(BlackboxSchedulerResult roundInfo)
        {
            _logger.LogInformation("Generating Round");

            Stopwatch timer = Stopwatch.StartNew();
            var possibleMatches = GetPossibleMatches().ToList();
            var bestMatches = GenerateBestMatches(possibleMatches);
            bestMatches = bestMatches is null ? new List<BasicMatch>() : bestMatches.ToList();
            timer.Stop();

            _logger.LogInformation($"Generated round in {timer.ElapsedMilliseconds}ms");

            roundInfo.PossibleMatches.AddRange(possibleMatches.Select(m => new BlackboxMatch() {
                Team1 = new(m.Team1.Id, m.Team1.SchedulingTeamValue),
                Team2 = new(m.Team2.Id, m.Team2.SchedulingTeamValue),
                Suitability = m.Suitability ?? 0
            }));

            roundInfo.ScheduledMatches.AddRange(bestMatches.Select(m => new BlackboxMatch()
            {
                Team1 = new(m.Team1.Id, m.Team1.SchedulingTeamValue),
                Team2 = new(m.Team2.Id, m.Team2.SchedulingTeamValue),
                Suitability = m.Suitability ?? 0
            }));

            return bestMatches;
        }

        private bool ActivatedForBlackbox(Team t)
        {
            return string.Equals(t.Division, "Competitive")
                && (t.LfgMode == LfgMode.Mixed || t.LfgMode == LfgMode.Strict)
                && t.Tournament?.Opponents.Any() != null;
        }

        private List<BasicMatch>? GenerateBestMatches(List<BasicMatch> possibleMatches)
        {
            var maxSuitability = -1;
            List<BasicMatch>? bestMatches = null;
            for (int i=0; i < possibleMatches.Count; i++)
            {
                var potentialMatch = possibleMatches[i];
                var remainingMatches = possibleMatches.Where(m => !CoachesCollide(potentialMatch, m)).ToList();
                var bestSubset = GenerateBestMatches(remainingMatches);
                if (bestSubset is not null)
                {
                    var suitability = bestSubset.Sum(m => CalculateSuitability(m)) + CalculateSuitability(potentialMatch);

                    if (suitability > maxSuitability)
                    {
                        bestMatches = bestSubset;
                        bestMatches.Add(potentialMatch);
                    }
                }
                else
                {
                    bestMatches = new() { potentialMatch };
                }
            }

            return bestMatches;
        }

        private bool CoachesCollide(BasicMatch potentialMatch, BasicMatch m)
        {
            return potentialMatch.Team1.Coach.Equals(m.Team1.Coach)
                || potentialMatch.Team1.Coach.Equals(m.Team2.Coach)
                || potentialMatch.Team2.Coach.Equals(m.Team1.Coach)
                || potentialMatch.Team2.Coach.Equals(m.Team2.Coach);
        }

        private IEnumerable<BasicMatch> GetPossibleMatches()
        {
            HashSet<(Coach, Coach)> processed = new();
            foreach (var coach in _matchGraph.GetCoaches())
            {
                var coachMatches = _matchGraph.GetMatches(coach);
                foreach (var match in coachMatches)
                {
                    match.Suitability = CalculateSuitability(match);
                }

                var opponents = coachMatches.GroupBy(m => m.GetOpponent(coach));
                foreach (var opponent in opponents)
                {
                    if (opponent.Key is not null && !processed.Contains((opponent.Key, coach)) && !processed.Contains((coach, opponent.Key)))
                    {
                        var bestMatch = opponent.MaxBy(m => m.Suitability);
                        if (bestMatch is not null)
                        {
                            yield return bestMatch;
                        }
                        processed.Add((coach, opponent.Key));
                    }
                }
            }
        }

        private int CalculateSuitability(BasicMatch match)
        {
            return CalculateSuitability(match.Team1, match.Team2);
        }

        private int CalculateSuitability(Team team1, Team team2)
        {
            // Calculate normalized TV difference
            var tvMin = (double) Math.Min(team1.SchedulingTeamValue, team2.SchedulingTeamValue);
            var tvMax = (double) Math.Max(team1.SchedulingTeamValue, team2.SchedulingTeamValue);
            var deltaTV = 1000 * (tvMax / tvMin - 1);

            // Amplify TV delta
            deltaTV = deltaTV <= 50 ? deltaTV : deltaTV * 3 - 100;

            // Calculate Win Probability
            var p = 1 / (Math.Pow(10, deltaTV / 700) + 1);

            var distance = Math.Abs(p - 0.5);

            // Apply small random factor and normalize
            distance = (distance + _random.NextDouble() * 0.02) / 0.52;

            // Calculate suitability
            var suitability = 1000 * (1 - distance);
            var repeatOpponentFactor = (team1.LastOpponent == team2.Coach.Id || team2.LastOpponent == team1.Coach.Id) ? 0.9 : 1;
            var rookieProtectionFactor = team1.Season != team2.Season && (team1.Season == 1 || team2.Season == 1) ? 0.8 : 1;

            return (int) (suitability * repeatOpponentFactor * rookieProtectionFactor);
        }
    }
}
