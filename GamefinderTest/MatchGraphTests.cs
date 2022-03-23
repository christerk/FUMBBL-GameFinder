using Fumbbl.Gamefinder.Model;
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
        private GamefinderFixture _fixture;
        private MatchGraph _graph;

        public MatchGraphTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
            _graph = new();
            GamefinderModel model = new(_graph);
            _graph.Start();
        }

        [Fact]
        public async void CoachAdded()
        {
            var coach = _fixture.CreateCoach(1);
            _ = _graph.AddAsync(coach);
            var coaches = await _graph.GetCoachesAsync();
            Assert.Contains(coach, coaches);
        }

        [Fact]
        public async void CoachRemoved()
        {
            var coach = _fixture.CreateCoach(1);
            _ = _graph.AddAsync(coach);
            _ = _graph.RemoveAsync(coach);
            var coaches = await _graph.GetCoachesAsync();
            Assert.DoesNotContain(coach, coaches);
        }

        [Fact]
        public async void TeamAdded()
        {
            var team = _fixture.SimpleTeam(1);
            _ = _graph.AddAsync(team);
            var teams = await _graph.GetTeamsAsync();
            Assert.Contains(team, teams);
        }

        [Fact]
        public async void TeamRemoved()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);
            _ = _graph.RemoveAsync(team1);
            var teams = await _graph.GetTeamsAsync();
            Assert.DoesNotContain(team1, teams);

            var matches = await _graph.GetMatchesAsync();
            Assert.DoesNotContain(new BasicMatch(team1, team2), matches);
        }

        [Fact]
        public async void MatchAdded()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);

            var expectedMatch = new BasicMatch(team1, team2);

            var matches = await _graph.GetMatchesAsync();
            Assert.True(matches.Count() == 1);
            Assert.Contains(expectedMatch, matches);
        }

        [Fact]
        public async void Foo()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);
            _ = _graph.AddTeamToCoachAsync(team1, team1.Coach);
            _ = _graph.AddTeamToCoachAsync(team2, team2.Coach);

            var expectedMatch = new BasicMatch(team1, team2);

            var matches = await _graph.GetMatchesAsync(team1.Coach);
            Assert.True(matches.Count() == 1);
            Assert.Contains(expectedMatch, matches);

        }

        [Fact]
        public async void MatchRemoved()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);

            var match = new BasicMatch(team1, team2);

            await _graph.RemoveAsync(match);

            var matches = await _graph.GetMatchesAsync();
            Assert.True(matches.Count() == 0);
        }
        [Fact]
        public async void MatchLaunched()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            var team3 = _fixture.SimpleTeam(3, 1);
            var team4 = _fixture.SimpleTeam(4, 2);
            var team5 = _fixture.SimpleTeam(5);
            var team6 = _fixture.SimpleTeam(6);

            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);
            _ = _graph.AddAsync(team3);
            _ = _graph.AddAsync(team4);
            _ = _graph.AddAsync(team5);
            _ = _graph.AddAsync(team6);

            _ = _graph.AddTeamToCoachAsync(team1, team1.Coach);
            _ = _graph.AddTeamToCoachAsync(team2, team2.Coach);
            _ = _graph.AddTeamToCoachAsync(team3, team3.Coach);
            _ = _graph.AddTeamToCoachAsync(team4, team4.Coach);
            _ = _graph.AddTeamToCoachAsync(team5, team5.Coach);
            _ = _graph.AddTeamToCoachAsync(team6, team6.Coach);

            var match = await _graph.GetMatchAsync(team1, team2);

            Assert.NotNull(match);

            if (match == null) return; // Gets rid of IntelliSense warning
            
            await _graph.TriggerLaunchGameAsync(match);

            // Check locks
            Assert.True(team1.Coach.Locked);
            Assert.True(team2.Coach.Locked);
            Assert.False(team5.Coach.Locked);

            var dependentMatch = await _graph.GetMatchAsync(team1, team5);

            Assert.NotNull(dependentMatch);
            Assert.True(dependentMatch?.MatchState.IsHidden);

            var independentMatch = await _graph.GetMatchAsync(team5, team6);
            Assert.NotNull(independentMatch);
            Assert.False(independentMatch?.MatchState.IsHidden);
        }

        public void Dispose()
        {
            _graph.Stop();
            GC.SuppressFinalize(this);
        }

    }
}
