using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class TeamPullTests
    {
        private const int Puller = 1;
        private const int Fallen = 2;
        private const int Helper = 3;
        private const int SecondHelper = 4;
        private const int Precision = 3;

        private readonly FakeFooting _footing = new FakeFooting();
        private readonly TeamPull _team;
        private readonly RopeLink _main = new RopeLink(Puller, Fallen, 1);
        private readonly RopeLink _help = new RopeLink(Helper, Fallen, 2);
        private readonly RopeLink _secondHelp = new RopeLink(SecondHelper, Fallen, 3);
        private readonly List<RopeLink> _links = new List<RopeLink>();

        public TeamPullTests()
        {
            _team = new TeamPull(new RopeHandPolicy(), _footing);
            _links.Add(_main);
            _links.Add(_help);
            _links.Add(_secondHelp);
        }

        [Fact]
        public void AlonePuller_HasNoBonus()
        {
            Assert.Equal(0, _team.HelpersFor(_main, Puller, _links));
            Assert.Equal(1f, _team.Multiplier(0));
        }

        [Fact]
        public void HoldingTeammates_CountAsHelpers()
        {
            _help.EndOf(Helper).IsHeld = true;
            _secondHelp.EndOf(SecondHelper).IsHeld = true;

            Assert.Equal(2, _team.HelpersFor(_main, Puller, _links));
        }

        [Fact]
        public void HelperOnTheWall_DoesNotCount()
        {
            _help.EndOf(Helper).IsHeld = true;
            _footing.Set(Helper, Footing.Climbing);

            Assert.Equal(0, _team.HelpersFor(_main, Puller, _links));
        }

        [Fact]
        public void ItsOwnRope_IsNotAHelper()
        {
            _main.EndOf(Puller).IsHeld = true;

            Assert.Equal(0, _team.HelpersFor(_main, Puller, _links));
        }

        [Theory]
        [InlineData(1, 1.6f)]
        [InlineData(2, 2f)]
        [InlineData(3, 2.267f)]
        public void Bonus_ShrinksForEachNextHelper(int helpers, float multiplier)
        {
            Assert.Equal(multiplier, _team.Multiplier(helpers), Precision);
        }

        [Fact]
        public void SharedLoad_IsSplitByTheTeam()
        {
            _help.EndOf(Helper).IsHeld = true;

            Assert.Equal(10f / 1.6f, _team.SharedLoad(_main, Puller, _links, 10f), Precision);
        }
    }
}
