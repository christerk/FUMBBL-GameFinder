﻿namespace Fumbbl.Gamefinder.Model
{
    public class Match
    {
        private const int DEFAULT_TIMEOUT = 30;
        private const int LAUNCHED_TIMEOUT = 3;
        private const int HIDDEN_TIMEOUT = 120;

        private readonly MatchGraph _owningGraph;
        private readonly Team _team1;
        private readonly Team _team2;
        private DateTime _resetTimestamp;
        private readonly MatchState _matchState;

        public MatchState MatchState => _matchState;

        public Team Team1 => _team1;
        public Team Team2 => _team2;

        public bool IsDialogActive => MatchState.TriggerStartDialog && _owningGraph.IsDialogActive(this);

        public Match(MatchGraph owningGraph, Team team1, Team team2)
        {
            _matchState = new MatchState();
            _owningGraph = owningGraph;
            _team1 = team1;
            _team2 = team2;
        }

        public void Act(TeamAction action, Team? team = default)
        {
            var activeDialog = _owningGraph.IsDialogActive(this);
            int teamNumber = _team1.Equals(team) ? 1 : 2;
            var matchAction = (action, teamNumber, activeDialog) switch
            {
                (TeamAction.Timeout, _, _) => MatchAction.Timeout,
                (TeamAction.Cancel, _, _) => MatchAction.Cancel,
                (TeamAction.Accept, 1, _) =>  MatchAction.Accept1,
                (TeamAction.Accept, 2, _) => MatchAction.Accept2,
                (TeamAction.Start, 1, true) => MatchAction.Start1,
                (TeamAction.Start, 2, true) => MatchAction.Start2,
                (_, _, _) => MatchAction.None
            };
            
            if (matchAction == MatchAction.None)
            {
                return;
            }

            var changed = _matchState.Act(matchAction);

            if (changed && action != TeamAction.Timeout)
            {
                var timeoutSeconds = GetTimeout(_matchState);

                _resetTimestamp = DateTime.Now.AddSeconds(timeoutSeconds);

                if (_matchState.TriggerStartDialog)
                {
                    _owningGraph.TriggerStartDialog(this);
                }

                if (_matchState.TriggerLaunchGame)
                {
                    _owningGraph.TriggerLaunchGame(this);
                }
            }
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
                    _owningGraph.RemoveCoach(Team1.Coach);
                    _owningGraph.RemoveCoach(Team2.Coach);
                }
                else
                {
                    Act(TeamAction.Timeout);
                }
            }
        }

        public override string ToString()
        {
            return $"Match({_team1.Name} vs {_team2.Name})";
        }

        public bool Includes(Team team)
        {
            return _team1.Equals(team) || _team2.Equals(team);
        }

        internal Team? GetOpponent(Team team)
        {
            if (_team1.Equals(team))
            {
                return _team2;
            }
            if (_team2.Equals(team))
            {
                return _team1;
            }
            return default;
        }
    }
}