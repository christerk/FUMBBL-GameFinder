﻿using Fumbbl.Gamefinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamefinderTest
{
    public class MatchStateTests : IClassFixture<GamefinderFixture>
    {
        private GamefinderFixture _fixture;

        public MatchStateTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Default()
        {
            var m = new MatchState();

            Assert.True(m.IsDefault);

            m.Act(Match(1, 2), MatchAction.Accept1);
            Assert.False(m.IsDefault);

            m = new MatchState();
            m.Act(Match(1, 2), MatchAction.Accept2);
            Assert.False(m.IsDefault);
        }

        [Fact]
        public void TriggerStart()
        {
            var match = Match(1,2);
            var m = new MatchState();

            Assert.False(m.TriggerStartDialog);
            m.Act(match, MatchAction.Accept1);
            Assert.False(m.TriggerStartDialog);
            m.Act(match, MatchAction.Accept2);
            Assert.True(m.TriggerStartDialog);
            m.Act(match, MatchAction.Start1);
            Assert.True(m.TriggerStartDialog);
            m.Act(match, MatchAction.Start2);
            Assert.False(m.TriggerStartDialog);

            m = new MatchState();
            m.Act(match, MatchAction.Accept2);
            m.Act(match, MatchAction.Accept1);
            m.Act(match, MatchAction.Start2);
            Assert.True(m.TriggerStartDialog);
            m.Act(match, MatchAction.Cancel);
            Assert.False(m.TriggerStartDialog);
        }

        [Fact]
        public void TriggerLaunch()
        {
            var match = Match(1, 2);
            var m = new MatchState();

            Assert.False(m.TriggerLaunchGame);
            m.Act(match, MatchAction.Accept1);
            Assert.False(m.TriggerLaunchGame);
            m.Act(match, MatchAction.Accept2);
            Assert.False(m.TriggerLaunchGame);
            m.Act(match, MatchAction.Start1);
            Assert.False(m.TriggerLaunchGame);
            m.Act(match, MatchAction.Start2);
            Assert.True(m.TriggerLaunchGame);

            m = new MatchState();
            m.Act(match, MatchAction.Accept2);
            m.Act(match, MatchAction.Accept1);
            m.Act(match, MatchAction.Start2);
            m.Act(match, MatchAction.Start1);
            Assert.True(m.TriggerLaunchGame);
        }

        [Fact]
        public void Hidden()
        {
            var match = Match(1,2);
            var m = new MatchState();

            Assert.False(m.IsHidden);
            m.Act(match, MatchAction.Cancel);
            Assert.True(m.IsHidden);
            m.Act(match, MatchAction.Accept1);
            Assert.True(m.IsHidden);
        }
        [Fact]
        public void Timeout()
        {
            var match = Match(1, 2);
            var m = new MatchState();

            m.Act(match, MatchAction.Accept1);
            m.Act(match, MatchAction.Timeout);
            Assert.True(m.IsDefault);
        }

        [Fact]
        public void ErroneousActions()
        {
            var match = Match(1, 2);
            var m = new MatchState();

            m.Act(match, MatchAction.Start2);
            Assert.True(m.IsDefault);
        }

        private BasicMatch Match(int id1, int id2)
        {
            return new BasicMatch(
                new Team(new Coach() { Id = id1 }) { Id = id1 },
                new Team(new Coach() { Id = id2 }) { Id = id2 }
            );
        }
    }
}
