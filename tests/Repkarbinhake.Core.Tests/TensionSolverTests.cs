using System.Numerics;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class TensionSolverTests
    {
        private const int Precision = 4;
        private const float Length = 5f;

        private readonly TestSettings _settings = new TestSettings();
        private readonly TensionSolver _solver;

        public TensionSolverTests()
        {
            _solver = new TensionSolver(_settings);
        }

        [Theory]
        [InlineData(1f)]
        [InlineData(4.99f)]
        [InlineData(5f)]
        public void SlackRope_DoesNotPull(float distance)
        {
            Vector3 pull = SolveStraight(Vector3.Zero, Vector3.Zero, new Vector3(distance, 0f, 0f), Length, 0f);

            Assert.Equal(Vector3.Zero, pull);
        }

        [Fact]
        public void StretchedRope_PullsTowardPartnerProportionallyToStretch()
        {
            Vector3 pull = SolveStraight(Vector3.Zero, Vector3.Zero, new Vector3(0f, 6f, 0f), Length, 0f);

            Assert.Equal(0f, pull.X, Precision);
            Assert.Equal(30f, pull.Y, Precision);
        }

        [Fact]
        public void MovingAway_AddsDamping()
        {
            Vector3 movingAway = new Vector3(-2f, 0f, 0f);

            Vector3 pull = SolveStraight(Vector3.Zero, movingAway, new Vector3(6f, 0f, 0f), Length, 0f);

            Assert.Equal(30f + 6f * 2f, pull.X, Precision);
        }

        [Fact]
        public void MovingToward_HasNoDamping()
        {
            Vector3 movingToward = new Vector3(3f, 0f, 0f);

            Vector3 pull = SolveStraight(Vector3.Zero, movingToward, new Vector3(6f, 0f, 0f), Length, 0f);

            Assert.Equal(30f, pull.X, Precision);
        }

        [Fact]
        public void HugeStretch_IsCappedByMaxPull()
        {
            Vector3 pull = SolveStraight(Vector3.Zero, Vector3.Zero, new Vector3(50f, 0f, 0f), Length, 0f);

            Assert.Equal(_settings.MaxPull, pull.Length(), Precision);
        }

        [Fact]
        public void Stability_ScalesPullDown()
        {
            Vector3 pull = SolveStraight(Vector3.Zero, Vector3.Zero, new Vector3(6f, 0f, 0f), Length, 0.8f);

            Assert.Equal(30f * 0.2f, pull.X, Precision);
        }

        [Fact]
        public void Load_IsZeroForSlackRope()
        {
            Assert.Equal(0f, LoadStraight(Vector3.Zero, new Vector3(4f, 0f, 0f), Length));
        }

        [Fact]
        public void Load_GrowsWithStretchAndIsCapped()
        {
            Assert.Equal(30f, LoadStraight(Vector3.Zero, new Vector3(6f, 0f, 0f), Length), Precision);
            Assert.Equal(_settings.MaxPull, LoadStraight(Vector3.Zero, new Vector3(50f, 0f, 0f), Length), Precision);
        }

        [Fact]
        public void IsSnapped_OnlyBeyondSnapDistance()
        {
            Assert.False(_solver.IsSnapped(Vector3.Zero, new Vector3(59f, 0f, 0f)));
            Assert.True(_solver.IsSnapped(Vector3.Zero, new Vector3(61f, 0f, 0f)));
        }

        [Fact]
        public void RopeOverEdge_PullsAlongRopeNotStraightToPartner()
        {
            RopeSpan overEdge = new RopeSpan(Vector3.UnitY, 6f);

            Vector3 pull = _solver.Solve(overEdge, Vector3.Zero, Length, 0f);

            Assert.Equal(0f, pull.X, Precision);
            Assert.Equal(30f, pull.Y, Precision);
        }

        [Fact]
        public void RopeOverEdge_StretchUsesPathLength()
        {
            RopeSpan straightShort = RopeSpan.Straight(Vector3.Zero, new Vector3(4f, 0f, 0f));
            RopeSpan wrappedLong = new RopeSpan(Vector3.UnitY, 6f);

            Assert.Equal(0f, _solver.Load(straightShort, Length));
            Assert.Equal(30f, _solver.Load(wrappedLong, Length), Precision);
        }

        [Fact]
        public void StraightSpan_PointsToPartnerWithDistance()
        {
            RopeSpan span = RopeSpan.Straight(Vector3.Zero, new Vector3(0f, 0f, 3f));

            Assert.Equal(Vector3.UnitZ, span.Direction);
            Assert.Equal(3f, span.PathLength, Precision);
        }

        private Vector3 SolveStraight(Vector3 self, Vector3 velocity, Vector3 partner, float length, float stability)
        {
            return _solver.Solve(RopeSpan.Straight(self, partner), velocity, length, stability);
        }

        private float LoadStraight(Vector3 self, Vector3 partner, float length)
        {
            return _solver.Load(RopeSpan.Straight(self, partner), length);
        }
    }
}
