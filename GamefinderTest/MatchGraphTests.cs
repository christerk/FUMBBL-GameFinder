using Fumbbl.Gamefinder.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamefinderTest
{
    public class MatchGraphTests : IDisposable, IClassFixture<GamefinderFixture>
    {
        private readonly GamefinderFixture _fixture;
        private readonly MatchGraph _graph;

        public MatchGraphTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
            _graph = _fixture.GamefinderModel.Graph;
        }

        [Fact]
        public void CoachAdded()
        {
            var coach = _fixture.CreateCoach(1);
            _graph.Add(coach);
            var coaches = _graph.GetCoaches();
            Assert.Contains(coach, coaches);
        }

        [Fact]
        public void CoachRemoved()
        {
            var coach = _fixture.CreateCoach(1);
            _graph.Add(coach);
            _graph.Remove(coach);
            var coaches = _graph.GetCoaches();
            Assert.DoesNotContain(coach, coaches);
        }

        [Fact]
        public void TeamAdded()
        {
            var team = _fixture.SimpleTeam(1);
            _graph.Add(team);
            var teams = _graph.GetTeams();
            Assert.Contains(team, teams);
        }

        [Fact]
        public void TeamRemoved()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _graph.Add(team1);
            _graph.Add(team2);
            _graph.Remove(team1);
            var teams = _graph.GetTeams();
            Assert.DoesNotContain(team1, teams);

            var matches = _graph.GetMatches();
            Assert.DoesNotContain(new BasicMatch(team1, team2), matches);
        }

        [Fact]
        public void MatchAdded()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _graph.Add(team1);
            _graph.Add(team2);

            var expectedMatch = new BasicMatch(team1, team2);

            var matches = _graph.GetMatches();
            Assert.Single(matches);
            Assert.Contains(expectedMatch, matches);
        }

        [Fact]
        public void GetMatchesForCoach()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _graph.Add(team1);
            _graph.Add(team2);

            var expectedMatch = new BasicMatch(team1, team2);

            var matches = _graph.GetMatches(team1.Coach);
            Assert.Single(matches);
            Assert.Contains(expectedMatch, matches);

        }

        [Fact]
        public void MatchRemoved()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _graph.Add(team1);
            _graph.Add(team2);

            var match = new BasicMatch(team1, team2);

            _graph.Remove(match);

            var matches = _graph.GetMatches();
            Assert.Empty(matches);
        }
        [Fact]
        public void MatchLaunched()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            var team3 = _fixture.SimpleTeam(3, team1.Coach);
            var team4 = _fixture.SimpleTeam(4, team2.Coach);
            var team5 = _fixture.SimpleTeam(5);
            var team6 = _fixture.SimpleTeam(6);

            _graph.Add(team1);
            _graph.Add(team2);
            _graph.Add(team3);
            _graph.Add(team4);
            _graph.Add(team5);
            _graph.Add(team6);

            var match = _graph.GetMatch(team1, team2);

            Assert.NotNull(match);

            if (match == null) return; // Gets rid of IntelliSense warning
            
            _graph.TriggerLaunchGame(match);

            // Check locks
            Assert.True(team1.Coach.Locked);
            Assert.True(team2.Coach.Locked);
            Assert.False(team5.Coach.Locked);

            var dependentMatch = _graph.GetMatch(team1, team5);

            Assert.NotNull(dependentMatch);
            Assert.True(dependentMatch?.MatchState.IsHidden);

            var independentMatch = _graph.GetMatch(team5, team6);
            Assert.NotNull(independentMatch);
            Assert.False(independentMatch?.MatchState.IsHidden);
        }

        [Fact]
        public void AddCoachesWithTeams()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);

            _graph.Add(team1);
            _graph.Add(team2);

            var match = _graph.GetMatch(team1, team2);
            Assert.NotNull(match);

            Assert.Single(_graph.GetMatches(team1.Coach));
            Assert.Single(_graph.GetMatches(team2.Coach));
        }

        [Fact]
        public void VerifyMatchStateAfterActivate()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);

            _graph.Add(team1);
            _graph.Add(team2);

            Assert.Single(_graph.GetMatches(team1.Coach));
            Assert.Single(_graph.GetMatches(team2.Coach));

            var match = _graph.GetMatch(team1, team2);
            Assert.NotNull(match);

            if (match == null)
            {
                return;
            }

            match.MatchState.Act(match, MatchAction.Accept2);

            _graph.Remove(team1.Coach);
            Assert.Empty(_graph.GetMatches(team2.Coach));
            _graph.Add(team1);

            var match1 = (_graph.GetMatches(team1.Coach)).Single();
            var match2 = (_graph.GetMatches(team2.Coach)).Single();

            Assert.True(match1.MatchState.IsDefault);
            Assert.True(match2.MatchState.IsDefault);
        }

        [Fact]
        public void MatchesForSingleCoach()
        {
            var team1 = _fixture.SimpleTeam(1);
            _graph.Add(team1);
            Assert.Empty(_graph.GetMatches(team1.Coach));
        }

        public void Dispose()
        {
            _graph.Reset();
            GC.SuppressFinalize(this);
        }

    }
}
