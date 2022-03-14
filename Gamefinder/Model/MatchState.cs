﻿namespace Fumbbl.Gamefinder.Model
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

        private static Action? ClearDialog(Match match) => () => match.Graph.ClearDialog(match);
        private static Action? TriggerStart(Match match) => () => match.Graph.TriggerStartDialog(match);
        private static Action? TriggerLaunch(Match match) => () => match.Graph.TriggerLaunchGame(match);

        public bool Act(Match match, MatchAction action)
        {
            (TeamState new1, TeamState new2, Action? trigger) = (State1, State2, action) switch
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

            trigger?.Invoke();

            return true;
        }
    }
}
