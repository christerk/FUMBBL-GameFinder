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

        private Func<Task>? ClearDialog(BasicMatch match) => async () => await match.ClearDialogAsync();
        private Func<Task>? TriggerStart(BasicMatch match) => async () => await match.TriggerStartAsync();
        private Func<Task>? TriggerLaunch(BasicMatch match) => async () => await match.TriggerLaunchAsync();

        public async Task<bool> ActAsync(BasicMatch match, MatchAction action)
        {
            (TeamState new1, TeamState new2, Func<Task>? trigger) = (State1, State2, action) switch
            {
                (_, _, MatchAction.Timeout) => (TeamState.Default, TeamState.Default, ClearDialog(match)),
                (TeamState.Hidden, TeamState.Hidden, _) => (TeamState.Hidden, TeamState.Hidden, null), // Stop pattern matching
                (_, _, MatchAction.Cancel) => (TeamState.Hidden, TeamState.Hidden, ClearDialog(match)),

                (TeamState.Default, TeamState.Default, MatchAction.Accept1) => (TeamState.Accept, TeamState.Default, null),
                (TeamState.Default, TeamState.Default, MatchAction.Accept2) => (TeamState.Default, TeamState.Accept, null),
                (TeamState.Accept, TeamState.Default, MatchAction.Accept2) => (TeamState.Accept, TeamState.Accept, TriggerStart(match)),
                (TeamState.Default, TeamState.Accept, MatchAction.Accept1) => (TeamState.Accept, TeamState.Accept, TriggerStart(match)),

                (TeamState.Accept, TeamState.Accept, MatchAction.Start1) => (TeamState.Start, TeamState.Accept, null),
                (TeamState.Accept, TeamState.Accept, MatchAction.Start2) => (TeamState.Accept, TeamState.Start, null),
                (TeamState.Start, TeamState.Accept, MatchAction.Start2) => (TeamState.Start, TeamState.Start, TriggerLaunch(match)),
                (TeamState.Accept, TeamState.Start, MatchAction.Start1) => (TeamState.Start, TeamState.Start, TriggerLaunch(match)),

                (_, _, _) => (State1, State2, null)
            };

            if ((State1, State2) == (new1, new2))
            {
                return false;
            }

            (State1, State2) = (new1, new2);

            if (trigger != null)
            {
                await trigger.Invoke();
            }

            return true;
        }
    }
}
