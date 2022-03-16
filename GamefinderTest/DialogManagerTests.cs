using Fumbbl.Gamefinder.Model;
using System;
using Xunit;

namespace GamefinderTest
{
    public class DialogManagerTests : IClassFixture<GamefinderFixture>
    {
        private GamefinderFixture _fixture;
        private DialogManager _dialogManager;

        public DialogManagerTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
            _dialogManager = new DialogManager();
        }

        [Fact]
        public void SingleDialogIsActive()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));

            _dialogManager.Add(m1);

            Assert.True(_dialogManager.IsDialogActive(m1));
        }

        [Fact]
        public void IndependentDialogsAreActive()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));
            BasicMatch m2 = new BasicMatch(GetTeam(3), GetTeam(4));

            _dialogManager.Add(m1);
            _dialogManager.Add(m2);

            Assert.True(_dialogManager.IsDialogActive(m1));
            Assert.True(_dialogManager.IsDialogActive(m2));
        }

        [Fact]
        public void DependentDialogsBlock()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));
            BasicMatch m2 = new BasicMatch(GetTeam(2), GetTeam(3));

            _dialogManager.Add(m1);
            _dialogManager.Add(m2);

            Assert.True(_dialogManager.IsDialogActive(m1));
            Assert.False(_dialogManager.IsDialogActive(m2));
        }

        [Fact]
        public void ActivateAfterRemove()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));
            BasicMatch m2 = new BasicMatch(GetTeam(2), GetTeam(3));

            _dialogManager.Add(m1);
            _dialogManager.Add(m2);

            Assert.True(_dialogManager.IsDialogActive(m1));
            Assert.False(_dialogManager.IsDialogActive(m2));

            _dialogManager.Remove(m1);
            Assert.True(_dialogManager.IsDialogActive(m2));
        }

        [Fact]
        public void NoDialogNonExistingMatch()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));
            BasicMatch m2 = new BasicMatch(GetTeam(2), GetTeam(3));
            _dialogManager.Add(m1);

            Assert.False(_dialogManager.IsDialogActive(m2));
        }

        [Fact]
        public void NoRescanIfSpecified()
        {
            BasicMatch m1 = new BasicMatch(GetTeam(1), GetTeam(2));
            BasicMatch m2 = new BasicMatch(GetTeam(2), GetTeam(3));

            _dialogManager.Add(m1);
            _dialogManager.Add(m2);

            Assert.True(_dialogManager.IsDialogActive(m1));
            Assert.False(_dialogManager.IsDialogActive(m2));

            _dialogManager.Remove(m1, false);
            Assert.False(_dialogManager.IsDialogActive(m2));
        }

        [Fact]
        public void RemovedCoachRemovesMatch()
        {
            var team1 = GetTeam(1);
            var team2 = GetTeam(2);
            BasicMatch m1 = new BasicMatch(team1, team2);

            _dialogManager.Add(m1);
            _dialogManager.Remove(team1.Coach);

            Assert.False(_dialogManager.IsDialogActive(m1));

            _dialogManager.Add(m1);
            _dialogManager.Remove(team2.Coach);
            Assert.False(_dialogManager.IsDialogActive(m1));
        }

        [Fact]
        public void RemovedTeamRemovesMatch()
        {
            var team1 = GetTeam(1);
            var team2 = GetTeam(2);
            BasicMatch m1 = new BasicMatch(team1, team2);

            _dialogManager.Add(m1);
            _dialogManager.Remove(team1);

            Assert.False(_dialogManager.IsDialogActive(m1));

            _dialogManager.Add(m1);
            _dialogManager.Remove(team2);
            Assert.False(_dialogManager.IsDialogActive(m1));
        }

        [Fact]
        public void RemovedCoachUnlocksOpponent()
        {
            var team1 = GetTeam(1);
            var team2 = GetTeam(2);
            BasicMatch match = new BasicMatch(team1, team2);

            _dialogManager.Add(match);

            _ = match.MatchState.ActAsync(match, MatchAction.Accept1);
            _ = match.MatchState.ActAsync(match, MatchAction.Accept2);
            _ = match.MatchState.ActAsync(match, MatchAction.Start1);
            _ = match.MatchState.ActAsync(match, MatchAction.Start2);

            _dialogManager.Remove(team1.Coach);

            Assert.False(_dialogManager.IsDialogActive(match));

            _dialogManager.Add(match);
            _dialogManager.Remove(team2);
            Assert.False(_dialogManager.IsDialogActive(match));
        }

        private Team GetTeam(int coach, int team = 0)
        {
            return _fixture.Teams[(coach % _fixture.Coaches.Count) * 3 + (team % 3)];
        }
    }
}