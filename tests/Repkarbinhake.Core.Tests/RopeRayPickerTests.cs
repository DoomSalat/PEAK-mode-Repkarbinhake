using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeRayPickerTests
    {
        private const int Precision = 3;
        private const float Reach = 3f;
        private const float Tolerance = 0.2f;

        private readonly RopeRayPicker _picker = new RopeRayPicker();
        private readonly List<Vector3> _rope = new List<Vector3>
        {
            new Vector3(-2f, 0f, 2f),
            new Vector3(0f, 0f, 2f),
            new Vector3(2f, 0f, 2f),
        };

        [Fact]
        public void LookingStraightAtRope_HitsAtItsDistance()
        {
            bool hit = _picker.TryPick(Vector3.Zero, Vector3.UnitZ, Reach, Tolerance, _rope, out float distance);

            Assert.True(hit);
            Assert.Equal(2f, distance, Precision);
        }

        [Fact]
        public void SlightlyOffAim_StillHitsWithinTolerance()
        {
            Vector3 direction = Vector3.Normalize(new Vector3(0f, 0.05f, 1f));

            Assert.True(_picker.TryPick(Vector3.Zero, direction, Reach, Tolerance, _rope, out _));
        }

        [Fact]
        public void LookingAway_Misses()
        {
            Assert.False(_picker.TryPick(Vector3.Zero, Vector3.UnitY, Reach, Tolerance, _rope, out _));
        }

        [Fact]
        public void RopeBehindViewer_Misses()
        {
            Assert.False(_picker.TryPick(Vector3.Zero, -Vector3.UnitZ, Reach, Tolerance, _rope, out _));
        }

        [Fact]
        public void RopeBeyondReach_Misses()
        {
            Assert.False(_picker.TryPick(Vector3.Zero, Vector3.UnitZ, 1.5f, Tolerance, _rope, out _));
        }

        [Fact]
        public void PastRopeEnd_Misses()
        {
            Vector3 direction = Vector3.Normalize(new Vector3(3f, 0f, 2f));

            Assert.False(_picker.TryPick(Vector3.Zero, direction, Reach * 2f, Tolerance, _rope, out _));
        }

        [Fact]
        public void ParallelRay_AlongRope_HitsNearestPoint()
        {
            List<Vector3> rope = new List<Vector3> { new Vector3(0f, 0.1f, 1f), new Vector3(0f, 0.1f, 4f) };

            Assert.True(_picker.TryPick(Vector3.Zero, Vector3.UnitZ, Reach, Tolerance, rope, out float distance));
            Assert.Equal(1f, distance, Precision);
        }

        [Fact]
        public void TooFewPoints_Misses()
        {
            List<Vector3> rope = new List<Vector3> { new Vector3(0f, 0f, 2f) };

            Assert.False(_picker.TryPick(Vector3.Zero, Vector3.UnitZ, Reach, Tolerance, rope, out _));
        }
    }
}
