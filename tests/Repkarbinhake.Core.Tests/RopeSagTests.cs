using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Tests.Fakes;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeSagTests
    {
        private const float Radius = 0.05f;
        private const float DeltaTime = 0.02f;
        private const int Bones = 20;

        private static readonly Vector3 Down = -Vector3.UnitY; // Static: shared constant for all tests
        private static readonly Vector3 Left = new Vector3(0f, 2f, 0f); // Static: shared constant for all tests
        private static readonly Vector3 Right = new Vector3(4f, 2f, 0f); // Static: shared constant for all tests

        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly RopeSag _sag = new RopeSag();
        private readonly BoxObstacles _open = new BoxObstacles();
        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly List<float> _freedoms = new List<float>();
        private readonly List<float> _spacings = new List<float>();

        [Fact]
        public void TautRope_LiesOnThePath()
        {
            Build(Left, Right, 4f, _open);

            foreach (Vector3 point in _points)
                Assert.Equal(2f, point.Y, 4);
        }

        [Fact]
        public void TautRope_HasAlmostNoFreedom()
        {
            Build(Left, Right, 4f, _open);

            foreach (float freedom in _freedoms)
                Assert.True(freedom <= RopeSag.TautFreedom + 0.001f);
        }

        [Fact]
        public void SlackRope_SagsDownInTheMiddle()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.True(_points[_points.Count / 2].Y < 2f - 0.3f);
        }

        [Fact]
        public void SlackRope_KeepsItsLength()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.InRange(Length(), 4.5f * 0.97f, 4.5f * 1.03f);
        }

        [Fact]
        public void LongSlack_LiesOnFloorInsteadOfSinking()
        {
            BoxObstacles floor = new BoxObstacles().Add(new Vector3(-10f, -5f, -10f), new Vector3(10f, 0f, 10f));
            Build(new Vector3(0f, 1f, 0f), new Vector3(4f, 1f, 0f), 12f, floor);

            foreach (Vector3 point in _points)
                Assert.True(point.Y >= Radius - 0.001f);
        }

        [Fact]
        public void Ends_ArePinned()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.Equal(Left, _points[0]);
            Assert.Equal(Right, _points[_points.Count - 1]);
            Assert.Equal(0f, _freedoms[0]);
            Assert.Equal(0f, _freedoms[_freedoms.Count - 1]);
        }

        [Theory]
        [InlineData(4f, 4.2f)]
        [InlineData(4f, 6f)]
        [InlineData(2f, 12f)]
        public void SagDepth_MatchesRopeLength(float chord, float material)
        {
            Assert.Equal(material, RopeSag.ParabolaLength(chord, RopeSag.SagDepth(chord, material)), 2);
        }

        private void Build(Vector3 start, Vector3 end, float ropeLength, BoxObstacles obstacles)
        {
            RopeLengthState length = new RopeLengthState(ropeLength, 0f, 0f);
            _path.Update(start, end, length, Radius, DeltaTime, obstacles);
            _sag.Build(_path, Down, Bones, Radius, obstacles, _points, _freedoms, _spacings);
        }

        private float Length()
        {
            float total = 0f;

            for (int i = 1; i < _points.Count; i++)
                total += Vector3.Distance(_points[i - 1], _points[i]);

            return total;
        }
    }
}
