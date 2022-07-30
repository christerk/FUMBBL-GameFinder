using Fumbbl.Gamefinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamefinderTest
{
    public class BlackboxModelTests : IDisposable, IClassFixture<GamefinderFixture>
    {
        private readonly GamefinderFixture _fixture;
        private readonly MatchGraph _graph;
        private readonly BlackboxModel _model;

        public BlackboxModelTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
            _model = _fixture.BlackboxModel;
            _graph = _model.Graph;
        }

        [Fact]
        public async void MatchesAreGenerated()
        {
            var team1 = _fixture.SimpleTeam(1);
            var team2 = _fixture.SimpleTeam(2);

            _model.Graph.Add(team1.Coach);
            _model.Graph.Add(team2.Coach);

            _model.Graph.Add(team1);
            _model.Graph.Add(team2);

            var matches = await _model.GenerateRound();
            Assert.NotEmpty(matches);
        }

        public void Dispose()
        {
            _graph.Reset();
            GC.SuppressFinalize(this);
        }
    }
}
