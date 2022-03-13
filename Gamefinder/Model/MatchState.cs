namespace Fumbbl.Gamefinder.Model
{
    public class MatchState
    {
        public TeamState State1 { get; private set; } = TeamState.Default;
        public TeamState State2 { get; private set; } = TeamState.Default;

        public bool TriggerStartDialog => (State1, State2) switch
        {
            (TeamState.Accept, TeamState.Accept) => true,
            (TeamState.Accept, TeamState.Start) => true,
            (TeamState.Start, TeamState.Accept) => true,
            _ => false
        };

        public bool TriggerLaunchGame => (State1, State2) == (TeamState.Start, TeamState.Start);
        public bool IsHidden => State1 == TeamState.Hidden;

        public bool IsDefault => State1 == TeamState.Default && State2 == TeamState.Default;

        public bool Act(MatchAction action) {
            (TeamState new1, TeamState new2) = (State1, State2, action) switch
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

                (_, _, _) => (State1, State2)
            };

            if ((State1, State2) == (new1, new2))
            {
                return false;
            }

            (State1, State2) = (new1, new2);

            return true;
        }
    }
}
