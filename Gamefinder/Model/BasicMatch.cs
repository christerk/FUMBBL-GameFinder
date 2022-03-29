namespace Fumbbl.Gamefinder.Model
{
    public class BasicMatch : IEquatable<BasicMatch>
    {
        protected readonly Team _team1;
        protected readonly Team _team2;
        protected readonly MatchState _matchState;

        public Team Team1 => _team1;
        public Team Team2 => _team2;
        public MatchState MatchState => _matchState;

        public bool BlackboxPrioritized { get; set; } = false;

        public BasicMatch(Team team1, Team team2)
        {
            // Ensure team1 is always the team with the lowest ID to allow mirror matches to be .Equals() == true
            (_team1, _team2) =
                (team1.Id <= team2.Id)
                ? (team1, team2)
                : (team2, team1);

            _matchState = new MatchState();
        }

        public bool Includes(Team team)
        {
            return _team1.Equals(team) || _team2.Equals(team);
        }

        public Team? GetOpponent(Team team)
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

        public virtual void TriggerLaunch() { }
        public virtual void TriggerStart() { }
        public virtual void ClearDialog() { }

        public override string ToString()
        {
            return $"Match({_team1.Name} vs {_team2.Name})";
        }


        public bool Equals(BasicMatch? other)
        {
            return other is not null && _team1.Equals(other._team1) && _team2.Equals(other._team2);
        }

        public override bool Equals(object? other)
        {
            return other is not null && other is BasicMatch match && Equals(match);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine("Match", _team1.Id, _team2.Id);
        }

        public bool IsBetween(int myTeamId, int opponentTeamId)
        {
            return myTeamId == _team1.Id && opponentTeamId == _team2.Id
                || myTeamId == _team2.Id && opponentTeamId == _team1.Id;
        }

        public bool IsAwaitingResponse(Coach coach)
        {
            var home = _team1.Coach.Equals(coach);

            return (home, _matchState.State1, _matchState.State2) switch
            {
                (true, TeamState.Default, TeamState.Accept) => true,
                (false, TeamState.Accept, TeamState.Default) => true,
                (true, TeamState.Accept, TeamState.Start) => true,
                (false, TeamState.Start, TeamState.Accept) => true,
                _ => false
            };
        }

        internal IEnumerable<string> CoachNamesStarted()
        {
            var coachNames = new List<string>();

            if (_matchState.State1.Equals(TeamState.Start))
            {
                coachNames.Add(Team1.Coach.Name);
            }

            if (_matchState.State2.Equals(TeamState.Start))
            {
                coachNames.Add(Team2.Coach.Name);
            }

            return coachNames;
        }
    }
}