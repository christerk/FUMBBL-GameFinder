using Fumbbl.Gamefinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamefinderTest
{
    public class GamefinderModelTests : IDisposable, IClassFixture<GamefinderFixture>
    {
        private readonly GamefinderFixture _fixture;
        private readonly MatchGraph _graph;
        private readonly GamefinderModel _model;

        public GamefinderModelTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
            _model = _fixture.GamefinderModel;
            _graph = _fixture.MatchGraph;
        }

        [Fact]
        public void LaunchedGameCancelsDialogs()
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

            var match1 = _graph.GetMatch(team1, team2);
            var match2 = _graph.GetMatch(team1, team4);

            Assert.NotNull(match1);
            Assert.NotNull(match2);

            if (match1 == null || match2 == null) return; // Gets rid of IntelliSense warning

            match1.MatchState.Act(match1, MatchAction.Accept1);
            match1.MatchState.Act(match1, MatchAction.Accept2);
            match2.MatchState.Act(match2, MatchAction.Accept1);
            match2.MatchState.Act(match2, MatchAction.Accept2);

            Assert.True(_graph.DialogManager.Contains(match1));
            Assert.True(_graph.DialogManager.Contains(match2));

            match1.MatchState.Act(match1, MatchAction.Start1);
            match1.MatchState.Act(match1, MatchAction.Start2);

            Assert.False(_graph.DialogManager.Contains(match1));
            Assert.False(_graph.DialogManager.Contains(match2));
        }

        public void Dispose()
        {
            _graph.Reset();
            GC.SuppressFinalize(this);
        }
    }
}
