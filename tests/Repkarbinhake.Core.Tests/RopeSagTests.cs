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
        private readonly List<Vector3> _output = new List<Vector3>();

        [Fact]
        public void TautRope_LiesOnThePath()
        {
            Build(Left, Right, 4f, _open);

            foreach (Vector3 point in _output)
                Assert.Equal(2f, point.Y, 4);
        }

        [Fact]
        public void SlackRope_SagsDownInTheMiddle()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.True(_output[_output.Count / 2].Y < 2f - 0.3f);
        }

        [Fact]
        public void SlackRope_KeepsRoughlyItsLength()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.InRange(Length(), 4.5f * 0.9f, 4.5f * 1.1f);
        }

        [Fact]
        public void LongSlack_LiesOnFloorInsteadOfSinking()
        {
            BoxObstacles floor = new BoxObstacles().Add(new Vector3(-10f, -5f, -10f), new Vector3(10f, 0f, 10f));
            Build(new Vector3(0f, 1f, 0f), new Vector3(4f, 1f, 0f), 12f, floor);

            foreach (Vector3 point in _output)
                Assert.True(point.Y >= Radius - 0.001f);
        }

        [Fact]
        public void PathPoints_StayInTheOutput()
        {
            Build(Left, Right, 4.5f, _open);

            Assert.Equal(Left, _output[0]);
            Assert.Equal(Right, _output[_output.Count - 1]);
        }

        [Fact]
        public void MovingEnd_SwingsAndCalmsDown()
        {
            RopeLengthState length = new RopeLengthState(5f, 0f, 0f);
            Vector3 moved = Right + new Vector3(0f, 0f, 1.5f);
            Step(Left, Right, length, 50);
            Step(Left, moved, length, 1);
            float justAfter = SidewaysOfMiddle();
            Step(Left, moved, length, 400);
            float settled = SidewaysOfMiddle();

            Assert.True(justAfter > settled + 0.05f);
        }

        [Fact]
        public void RopeLyingOnFloor_DoesNotWanderSideways()
        {
            BoxObstacles floor = new BoxObstacles().Add(new Vector3(-10f, -5f, -10f), new Vector3(10f, 0f, 10f));
            RopeLengthState length = new RopeLengthState(8f, 0f, 0f);
            Vector3 start = new Vector3(0f, 0.3f, 0f);
            Vector3 end = new Vector3(3f, 0.3f, 0f);

            for (int i = 0; i < 60; i++)
            {
                Vector3 jiggle = new Vector3(0f, 0f, (i % 2) * 0.05f);
                _path.Update(start, end + jiggle, length, Radius, DeltaTime, floor);
                _sag.Build(_path, Down, Bones, Radius, DeltaTime, floor, _output);
            }

            foreach (Vector3 point in _output)
                Assert.InRange(point.Z, -0.1f, 0.15f);
        }

        private void Build(Vector3 start, Vector3 end, float ropeLength, BoxObstacles obstacles)
        {
            RopeLengthState length = new RopeLengthState(ropeLength, 0f, 0f);
            _path.Update(start, end, length, Radius, DeltaTime, obstacles);
            _sag.Build(_path, Down, Bones, Radius, DeltaTime, obstacles, _output);
        }

        private void Step(Vector3 start, Vector3 end, RopeLengthState length, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                _path.Update(start, end, length, Radius, DeltaTime, _open);
                _sag.Build(_path, Down, Bones, Radius, DeltaTime, _open, _output);
            }
        }

        private float SidewaysOfMiddle()
        {
            Vector3 middle = _output[_output.Count / 2];
            Vector3 start = _output[0];
            Vector3 end = _output[_output.Count - 1];
            Vector3 chord = Vector3.Normalize(end - start);
            Vector3 offset = middle - Vector3.Lerp(start, end, 0.5f);
            Vector3 horizontal = Vector3.Normalize(Vector3.Cross(chord, Vector3.UnitY));
            return System.MathF.Abs(Vector3.Dot(offset, horizontal));
        }

        private float Length()
        {
            float total = 0f;

            for (int i = 1; i < _output.Count; i++)
                total += Vector3.Distance(_output[i - 1], _output[i]);

            return total;
        }
    }
}
