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
            _graph = _fixture.MatchGraph;
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
            await _graph.AddAsync(coach);
            await _graph.RemoveAsync(coach);
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
            Assert.Single(matches);
            Assert.Contains(expectedMatch, matches);
        }

        [Fact]
        public async void GetMatchesForCoach()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);

            var expectedMatch = new BasicMatch(team1, team2);

            var matches = await _graph.GetMatchesAsync(team1.Coach);
            Assert.Single(matches);
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
            Assert.Empty(matches);
        }
        [Fact]
        public async void MatchLaunched()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);
            var team3 = _fixture.SimpleTeam(3, team1.Coach);
            var team4 = _fixture.SimpleTeam(4, team2.Coach);
            var team5 = _fixture.SimpleTeam(5);
            var team6 = _fixture.SimpleTeam(6);

            _ = _graph.AddAsync(team1);
            _ = _graph.AddAsync(team2);
            _ = _graph.AddAsync(team3);
            _ = _graph.AddAsync(team4);
            _ = _graph.AddAsync(team5);
            _ = _graph.AddAsync(team6);

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

        [Fact]
        public async void AddCoachesWithTeams()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);

            await _graph.AddAsync(team1);
            await _graph.AddAsync(team2);

            var match = await _graph.GetMatchAsync(team1, team2);
            Assert.NotNull(match);

            Assert.Single(await _graph.GetMatchesAsync(team1.Coach));
            Assert.Single(await _graph.GetMatchesAsync(team2.Coach));
        }

        [Fact]
        public async void VerifyMatchStateAfterActivate()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);

            await _graph.AddAsync(team1);
            await _graph.AddAsync(team2);

            Assert.Single(await _graph.GetMatchesAsync(team1.Coach));
            Assert.Single(await _graph.GetMatchesAsync(team2.Coach));

            var match = await _graph.GetMatchAsync(team1, team2);
            Assert.NotNull(match);

            if (match == null)
            {
                return;
            }

            await match.MatchState.ActAsync(match, MatchAction.Accept2);

            await _graph.RemoveAsync(team1.Coach);
            Assert.Empty(await _graph.GetMatchesAsync(team2.Coach));
            await _graph.AddAsync(team1);

            var match1 = (await _graph.GetMatchesAsync(team1.Coach)).Single();
            var match2 = (await _graph.GetMatchesAsync(team2.Coach)).Single();

            Assert.True(match1.MatchState.IsDefault);
            Assert.True(match2.MatchState.IsDefault);
        }

        [Fact]
        public async void MatchesForSingleCoach()
        {
            var team1 = _fixture.SimpleTeam(1);
            await _graph.AddAsync(team1);
            Assert.Empty(await _graph.GetMatchesAsync(team1.Coach));
        }

        public void Dispose()
        {
            _ = _graph.Reset();
            GC.SuppressFinalize(this);
        }

    }
}
