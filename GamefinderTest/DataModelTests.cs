using Fumbbl.Gamefinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GamefinderTest
{
    public class DataModelTests : IClassFixture<GamefinderFixture>
    {
        private GamefinderFixture _fixture;

        public DataModelTests(GamefinderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CoachEquality()
        {
            Coach c1 = new() { Id = 1, Name = "Coach 1" };
            Coach c2 = new() { Id = 2, Name = "Coach 2" };
            Coach c3 = new() { Id = 1, Name = "Another Coach 1" };

            Assert.Equal(c1, c1);
            Assert.NotEqual(c1, c2);
            Assert.Equal(c1, c3);
        }

        [Fact]
        public void CoachLock()
        {
            Coach c = new() { Id = 1, Name = "Coach 1" };

            Assert.False(c.Locked);
            c.Lock();
            Assert.True(c.Locked);
            c.Unlock();
            Assert.False(c.Locked);
        }

        [Fact]
        public void TeamEquality()
        {
            Coach c1 = new() { Id = 1, Name = "Coach 1" };
            Coach c2 = new() { Id = 2, Name = "Coach 2" };

            Team t1 = new(c1) { Id = 1 };
            Team t2 = new(c1) { Id = 2 };
            Team t3 = new(c1) { Id = 1 };

            Assert.Equal(t1, t1);
            Assert.NotEqual(t1, t2);
            Assert.Equal(t1, t3);

            Team t4 = new(c2) { Id = 1, Name = "Team of another coach, but with same ID" };
            Assert.Equal(t1, t4);
        }

        [Fact]
        public void MatchEquality()
        {
            Coach c1 = new() { Id = 1, Name = "Coach 1" };
            Coach c2 = new() { Id = 2, Name = "Coach 2" };
            Coach c3 = new() { Id = 3, Name = "Coach 3" };

            Team t1 = new Team(c1) { Id = 1 };
            Team t2 = new Team(c2) { Id = 2 };
            Team t3 = new Team(c2) { Id = 2 };

            BasicMatch m1 = new(t1, t2);
            BasicMatch m2 = new(t1, t3);

            Assert.NotNull(m1);
            Assert.NotNull(m2);

            Assert.Equal(m1, m2);
            Assert.False(m1.Equals(null));

            if (m1 == null) return; // Remove IntelliSense warning
            Assert.False(m1.Equals("Not a match"));
            Assert.True(m1.Equals(m1 as object));
        }

        [Fact]
        public void MirroredEquality()
        {
            Coach c1 = new() { Id = 1, Name = "Coach 1" };
            Coach c2 = new() { Id = 2, Name = "Coach 2" };

            Team t1 = new(c1) { Id = 1 };
            Team t2 = new(c2) { Id = 2 };

            BasicMatch m = new(t1, t2);
            BasicMatch mirrored = new(t2, t1);

            Assert.Equal(m, mirrored);
        }

        [Fact]
        public void MatchOpponents()
        {
            var c1 = new Coach() { Id = 1, Name = "Coach 1" };
            var c2 = new Coach() { Id = 2, Name = "Coach 2" };

            var t1 = new Team(c1) { Id = 1 };
            var t2 = new Team(c2) { Id = 2 };
            var t3 = new Team(c2) { Id = 3 };

            BasicMatch match = new BasicMatch(t1, t2);

            Assert.Equal(match.GetOpponent(t1), t2);
            Assert.Equal(match.GetOpponent(t2), t1);
            Assert.Null(match.GetOpponent(t3));
        }

        [Fact]
        public void MatchIncludes()
        {
            Coach c1 = new Coach() { Id = 1, Name = "Coach 1" };
            Coach c2 = new Coach() { Id = 2, Name = "Coach 2" };

            Team t1 = new Team(c1) { Id = 1 };
            Team t2 = new Team(c2) { Id = 2 };
            Team t3 = new Team(c2) { Id = 3 };

            BasicMatch match = new BasicMatch(t1, t2);

            Assert.True(match.Includes(t1));
            Assert.True(match.Includes(t2));
            Assert.False(match.Includes(t3));
        }

    }
}
