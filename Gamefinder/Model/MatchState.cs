namespace Fumbbl.Gamefinder.Model
{
    public class MatchState
    {
        private TeamState _state1 = TeamState.Default;
        private TeamState _state2 = TeamState.Default;

        public bool TriggerStartDialog => (_state1, _state2) == (TeamState.Accept, TeamState.Accept);
        public bool TriggerLaunchGame => (_state1, _state2) == (TeamState.Start, TeamState.Start);
        public bool IsHidden => _state1 == TeamState.Hidden;
        public bool Act(MatchAction action) {
            (TeamState new1, TeamState new2) = (_state1, _state2, action) switch
            {
                (_, _, MatchAction.Timeout) => (TeamState.Default, TeamState.Default),
                (TeamState.Hidden, TeamState.Hidden, _) => (TeamState.Hidden, TeamState.Hidden),
                (_, _, MatchAction.Cancel) => (TeamState.Hidden, TeamState.Hidden),

                (TeamState.Default, TeamState.Default, MatchAction.Accept1) => (TeamState.Accept, TeamState.Default),
                (TeamState.Default, TeamState.Default, MatchAction.Accept2) => (TeamState.Default, TeamState.Accept),
                (TeamState.Accept, TeamState.Default, MatchAction.Accept2) => (TeamState.Accept, TeamState.Accept),
                (TeamState.Default, TeamState.Accept, MatchAction.Accept1) => (TeamState.Accept, TeamState.Accept),

                (TeamState.Accept, TeamState.Accept, MatchAction.Start1) => (TeamState.Start, TeamState.Accept), 
                (TeamState.Accept, TeamState.Accept, MatchAction.Start2) => (TeamState.Accept, TeamState.Start),
                (TeamState.Start, TeamState.Accept, MatchAction.Start2) => (TeamState.Start, TeamState.Start),
                (TeamState.Accept, TeamState.Start, MatchAction.Start1) => (TeamState.Start, TeamState.Start),

                (_, _, _) => (_state1, _state2)
            };

            if ((_state1, _state2) == (new1, new2))
            {
                return false;
            }

            (_state1, _state2) = (new1, new2);

            return true;
        }
    }
}
