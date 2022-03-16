namespace Fumbbl.Gamefinder.Model
{
    public class Match : BasicMatch
    {
        private const int DEFAULT_TIMEOUT = 30;
        private const int LAUNCHED_TIMEOUT = 3;
        private const int HIDDEN_TIMEOUT = 120;

        private readonly MatchGraph _owningGraph;
        private DateTime _resetTimestamp;

        public MatchGraph Graph => _owningGraph;

        public bool IsDialogActive => MatchState.TriggerStartDialog && _owningGraph.IsDialogActive(this);

        public Match(MatchGraph owningGraph, Team team1, Team team2)
            : base(team1, team2)
        {
            _owningGraph = owningGraph;
        }

        public async Task ActAsync(TeamAction action, Team? team = default)
        {
            await _owningGraph.DispatchAsync(async () => await InternalAct(action, team));
        }

        private async Task InternalAct(TeamAction action, Team? team)
        {
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

            var changed = await _matchState.ActAsync(this, matchAction);

            if (changed && action != TeamAction.Timeout)
            {
                var timeoutSeconds = GetTimeout(_matchState);

                _resetTimestamp = DateTime.Now.AddSeconds(timeoutSeconds);
            }
        }

        public async Task TriggerStartDialogAsync()
        {
            await _owningGraph.TriggerStartDialogAsync(this);
        }

        public async Task TriggerLaunchGame()
        {
            await _owningGraph.TriggerLaunchGameAsync(this);
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

        public override async Task TriggerLaunchAsync()
        {
            await Graph.TriggerLaunchGameAsync(this);
        }
        public override async Task TriggerStartAsync()
        {
            await Graph.TriggerStartDialogAsync(this);
        }
        public override async Task ClearDialogAsync()
        {
            await Graph.ClearDialogAsync(this);
        }

        internal void Tick()
        {
            if (DateTime.Now > _resetTimestamp)
            {
                if (_matchState.TriggerLaunchGame)
                {
                    _ = _owningGraph.RemoveAsync(Team1.Coach);
                    _ = _owningGraph.RemoveAsync(Team2.Coach);
                }
                else
                {
                    _ = ActAsync(TeamAction.Timeout);
                }
            }
        }
    }
}