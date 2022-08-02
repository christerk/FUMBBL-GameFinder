namespace Fumbbl.Gamefinder.Model
{
    public class Match : BasicMatch
    {
        public const int DEFAULT_TIMEOUT = 60;
        public const int LAUNCHED_TIMEOUT = 30;
        public const int HIDDEN_TIMEOUT = 300;

        private readonly MatchGraph _owningGraph;
        private DateTime _resetTimestamp;

        private MatchGraph Graph => _owningGraph;

        public bool IsDialogActive => MatchState.TriggerStartDialog && _owningGraph.IsDialogActive(this);

        public long TimeUntilReset => (long)(_resetTimestamp - DateTime.Now).TotalMilliseconds;

        public Match(MatchGraph owningGraph, Team team1, Team team2)
            : base(team1, team2)
        {
            _owningGraph = owningGraph;
        }

        public void Act(TeamAction action, Team? team = default)
        {
            _owningGraph.Logger.LogDebug($"{this} action : {action} by {team}");
            var activeDialog = _owningGraph.IsDialogActive(this);
            int teamNumber = _team1.Equals(team) ? 1 : 2;
            var matchAction = (action, teamNumber, activeDialog) switch
            {
                (TeamAction.Timeout, _, _) => MatchAction.Timeout,
                (TeamAction.Cancel, _, _) => MatchAction.Cancel,
                (TeamAction.Accept, 1, _) => MatchAction.Accept1,
                (TeamAction.Accept, 2, _) => MatchAction.Accept2,
                (TeamAction.Start, 1, true) => MatchAction.Start1,
                (TeamAction.Start, 2, true) => MatchAction.Start2,
                (_, _, _) => MatchAction.None
            };

            if (matchAction == MatchAction.None)
            {
                return;
            }

            var changed = _matchState.Act(this, matchAction);

            if (changed && action != TeamAction.Timeout)
            {
                var timeoutSeconds = GetTimeout(_matchState);

                _resetTimestamp = DateTime.Now.AddSeconds(timeoutSeconds);
            }
        }

        public void TriggerStartDialogAsync()
        {
           _owningGraph.TriggerStartDialog(this);
        }

        public void TriggerLaunchGame()
        {
            _owningGraph.TriggerLaunchGame(this);
        }

        private static int GetTimeout(MatchState state)
        {
            if (state.IsHidden)
            {
                return HIDDEN_TIMEOUT;
            }

            if (state.TriggerLaunchGame)
            {
                return LAUNCHED_TIMEOUT;
            }

            return DEFAULT_TIMEOUT;
        }

        public override void TriggerLaunch()
        {
            Graph.TriggerLaunchGame(this);
        }
        public override void TriggerStart()
        {
            Graph.TriggerStartDialog(this);
        }
        public override void ClearDialog()
        {
            Graph.ClearDialog(this);
        }

        internal void Tick()
        {
            if (DateTime.Now > _resetTimestamp)
            {
                if (_matchState.TriggerLaunchGame)
                {
                    _owningGraph.Logger.LogDebug($"Removing started match {this}");
                    _owningGraph.Remove(Team1.Coach);
                    _owningGraph.Remove(Team2.Coach);
                }
                else if (!_matchState.IsDefault)
                {
                    Act(TeamAction.Timeout);
                }
            }
        }

        internal void ForceLaunch()
        {
            MatchState.ForceLaunch();
            _resetTimestamp = DateTime.Now.AddSeconds(GetTimeout(MatchState));
        }
    }
}