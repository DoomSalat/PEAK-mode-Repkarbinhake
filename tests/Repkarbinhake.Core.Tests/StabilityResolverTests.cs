using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class StabilityResolverTests
    {
        private const int Precision = 4;

        private readonly TestSettings _settings = new TestSettings();
        private readonly FakeFooting _footing = new FakeFooting();
        private readonly StabilityResolver _resolver;

        public StabilityResolverTests()
        {
            _resolver = new StabilityResolver(_settings, new RopeHandPolicy(), _footing);
        }

        [Fact]
        public void NotHoldingAnyRope_HasNoStability()
        {
            List<RopeLink> links = new List<RopeLink> { Link(1, 2, holder: 2) };

            Assert.Equal(0f, _resolver.Resolve(1, links));
        }

        [Fact]
        public void HoldingAlone_UsesBraceResistance()
        {
            List<RopeLink> links = new List<RopeLink> { Link(1, 2, holder: 1) };

            Assert.Equal(0.8f, _resolver.Resolve(1, links), Precision);
        }

        [Fact]
        public void AnotherBracingPlayerInGroup_AddsGroupBonus()
        {
            List<RopeLink> links = new List<RopeLink>
            {
                Link(1, 2, holder: 1),
                Link(2, 3, holder: 3),
            };

            Assert.Equal(1f - 0.2f * 0.75f, _resolver.Resolve(1, links), Precision);
        }

        [Fact]
        public void BracingPlayerInAnotherGroup_GivesNoBonus()
        {
            List<RopeLink> links = new List<RopeLink>
            {
                Link(1, 2, holder: 1),
                Link(3, 4, holder: 3),
            };

            Assert.Equal(0.8f, _resolver.Resolve(1, links), Precision);
        }

        [Fact]
        public void PlayerHoldingMyRope_PassesStabilityAlongChain()
        {
            List<RopeLink> links = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(2, 3, holder: 3),
            };

            float supporter = 0.8f;
            float chainLoose = 0.2f * (1f - 0.6f * supporter);
            Assert.Equal(1f - chainLoose * 0.75f, _resolver.Resolve(2, links), Precision);
        }

        [Fact]
        public void Chain_IsStrongerThanGroupBonusAlone()
        {
            List<RopeLink> chain = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(2, 3, holder: 3),
            };
            List<RopeLink> sideBySide = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(1, 3, holder: 3),
            };

            Assert.True(_resolver.Resolve(2, chain) > _resolver.Resolve(2, sideBySide));
        }

        [Fact]
        public void LongerChain_GivesMoreStabilityToFront()
        {
            List<RopeLink> shortChain = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(2, 3, holder: 3),
            };
            List<RopeLink> longChain = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(2, 3, holder: 3),
                Link(3, 4, holder: 4),
            };

            Assert.True(_resolver.Resolve(2, longChain) > _resolver.Resolve(2, shortChain));
        }

        [Fact]
        public void CyclicHolding_TerminatesWithinCap()
        {
            List<RopeLink> links = new List<RopeLink>
            {
                Link(1, 2, holder: 1),
                Link(2, 3, holder: 2),
                Link(1, 3, holder: 3),
            };

            float stability = _resolver.Resolve(1, links);

            Assert.InRange(stability, 0.8f, _settings.MaxStability);
        }

        [Fact]
        public void ManySupporters_AreCappedSoPlayerCanStillBeMoved()
        {
            List<RopeLink> links = new List<RopeLink> { Link(1, 2, holder: 1) };

            for (int actor = 3; actor <= 8; actor++)
                links.Add(Link(1, actor, holder: actor));

            Assert.Equal(_settings.MaxStability, _resolver.Resolve(1, links), Precision);
        }

        [Fact]
        public void BothHoldingSameRope_AreBothBracedAndSupportEachOther()
        {
            RopeLink link = Link(1, 2, holder: 1);
            link.EndOf(2).IsHeld = true;
            List<RopeLink> links = new List<RopeLink> { link };

            float stability = _resolver.Resolve(1, links);

            Assert.Equal(stability, _resolver.Resolve(2, links), Precision);
            Assert.True(stability > 0.8f);
        }

        [Theory]
        [InlineData(Footing.Airborne)]
        [InlineData(Footing.Climbing)]
        [InlineData(Footing.Unconscious)]
        public void HolderWithoutFooting_CannotBrace(Footing footing)
        {
            List<RopeLink> links = new List<RopeLink> { Link(1, 2, holder: 1) };
            _footing.Set(1, footing);

            Assert.Equal(0f, _resolver.Resolve(1, links));
        }

        [Fact]
        public void HangingPlayerHoldingRope_DoesNotWeakenPullOnHim()
        {
            RopeLink link = Link(1, 2, holder: 1);
            link.EndOf(2).IsHeld = true;
            List<RopeLink> links = new List<RopeLink> { link };
            _footing.Set(2, Footing.Airborne);

            Assert.Equal(0f, _resolver.Resolve(2, links));
            Assert.Equal(0.8f, _resolver.Resolve(1, links), Precision);
        }

        [Fact]
        public void AirborneSupporter_GivesNoChainSupport()
        {
            List<RopeLink> links = new List<RopeLink>
            {
                Link(1, 2, holder: 2),
                Link(2, 3, holder: 3),
            };
            _footing.Set(3, Footing.Airborne);

            Assert.Equal(0.8f, _resolver.Resolve(2, links), Precision);
        }

        private RopeLink Link(int first, int second, int holder)
        {
            RopeLink link = new RopeLink(first, second, 1L);
            link.EndOf(holder).IsHeld = true;
            return link;
        }
    }
}
