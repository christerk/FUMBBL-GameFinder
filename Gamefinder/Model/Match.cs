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

        public void Act(TeamAction action, Team? team = default)
        {
            _owningGraph.Dispatch(() => InternalAct(action, team));
        }

        private void InternalAct(TeamAction action, Team? team)
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

            var changed = _matchState.Act(this, matchAction);

            if (changed && action != TeamAction.Timeout)
            {
                var timeoutSeconds = GetTimeout(_matchState);

                _resetTimestamp = DateTime.Now.AddSeconds(timeoutSeconds);
            }
        }

        public void TriggerStartDialog()
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

        internal void Tick()
        {
            if (DateTime.Now > _resetTimestamp)
            {
                if (_matchState.TriggerLaunchGame)
                {
                    _owningGraph.Remove(Team1.Coach);
                    _owningGraph.Remove(Team2.Coach);
                }
                else
                {
                    Act(TeamAction.Timeout);
                }
            }
        }
    }
}