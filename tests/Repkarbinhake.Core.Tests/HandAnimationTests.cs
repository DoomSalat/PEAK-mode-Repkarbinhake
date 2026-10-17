using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class HandAnimationTests
    {
        private const int Precision = 4;
        private const float Stroke = 0.6f;
        private const float Near = 0.2f;
        private const float Far = 0.7f;

        private readonly HandOverHand _hands = new HandOverHand(Stroke, Near, Far);

        [Fact]
        public void AtRest_RightHandIsFarAndLeftHandIsNear()
        {
            Assert.Equal(Far, _hands.RightReach, Precision);
            Assert.Equal(Near, _hands.LeftReach, Precision);
        }

        [Fact]
        public void HalfStroke_SwapsHands()
        {
            _hands.Advance(Stroke / 2f);

            Assert.Equal(Near, _hands.RightReach, Precision);
            Assert.Equal(Far, _hands.LeftReach, Precision);
        }

        [Fact]
        public void FullStroke_ReturnsToStart()
        {
            _hands.Advance(Stroke);

            Assert.Equal(0f, _hands.Phase, Precision);
        }

        [Fact]
        public void ReelingOut_RunsCycleBackwards()
        {
            _hands.Advance(-Stroke / 4f);

            Assert.Equal(0.75f, _hands.Phase, Precision);
        }

        [Fact]
        public void Reach_AlwaysStaysBetweenNearAndFar()
        {
            for (int i = 0; i < 40; i++)
            {
                _hands.Advance(0.037f);
                Assert.InRange(_hands.RightReach, Near - 0.0001f, Far + 0.0001f);
                Assert.InRange(_hands.LeftReach, Near - 0.0001f, Far + 0.0001f);
            }
        }

        [Fact]
        public void NoReel_NoCoil()
        {
            List<Vector3> points = new List<Vector3>();

            new CoilShape(0.12f, 3f, 0.03f).Build(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, 0f, points);

            Assert.Empty(points);
        }

        [Fact]
        public void Coil_GrowsWithReelAndIsCapped()
        {
            CoilShape coil = new CoilShape(0.12f, 3f, 0.03f);

            Assert.True(coil.TurnsFor(1f) > coil.TurnsFor(0.5f));
            Assert.Equal(3f, coil.TurnsFor(100f), Precision);
        }

        [Fact]
        public void CoilPoints_LieOnRadiusAroundCenter()
        {
            List<Vector3> points = new List<Vector3>();
            float radius = 0.12f;

            new CoilShape(radius, 3f, 0f).Build(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, 0.5f, points);

            Assert.True(points.Count > 2);
            Vector3 center = -Vector3.UnitY * radius;

            foreach (Vector3 point in points)
                Assert.Equal(radius, Vector3.Distance(point, center), Precision);

            Assert.Equal(Vector3.Zero, points[0]);
        }
    }
}
