using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Tests.Fakes;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeLoosenessTests
    {
        private const float Radius = 0.05f;
        private const float DeltaTime = 0.02f;
        private const int Bones = 20;
        private const int Frames = 150;

        private static readonly Vector3 Down = -Vector3.UnitY; // Static: shared constant for all tests
        private static readonly Vector3 Gravity = new Vector3(0f, -9.81f, 0f); // Static: shared constant for all tests

        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly RopeSag _sag = new RopeSag();
        private readonly RopeLooseness _loose = new RopeLooseness();
        private readonly List<Vector3> _targets = new List<Vector3>();
        private readonly List<float> _freedoms = new List<float>();
        private readonly List<float> _spacings = new List<float>();
        private readonly BoxObstacles _floor = new BoxObstacles().Add(new Vector3(-10f, -5f, -10f), new Vector3(10f, 0f, 10f));

        [Fact]
        public void Points_StayWithinTheirFreedom()
        {
            Run(new Vector3(0f, 2f, 0f), new Vector3(4f, 2f, 0f), 6f, new BoxObstacles());

            for (int i = 0; i < _targets.Count; i++)
                Assert.True(Vector3.Distance(_loose.Positions[i], _targets[i]) <= _freedoms[i] + 0.001f);
        }

        [Fact]
        public void PinnedPoints_FollowTheirTargets()
        {
            Run(new Vector3(0f, 2f, 0f), new Vector3(4f, 2f, 0f), 6f, new BoxObstacles());

            Assert.Equal(_targets[0], _loose.Positions[0]);
            Assert.Equal(_targets[_targets.Count - 1], _loose.Positions[_targets.Count - 1]);
        }

        [Fact]
        public void LyingRope_StaysAboveTheFloor()
        {
            Run(new Vector3(0f, 0.4f, 0f), new Vector3(3f, 0.4f, 0f), 9f, _floor);

            foreach (Vector3 point in _loose.Positions)
                Assert.True(point.Y >= Radius - 0.001f);
        }

        [Fact]
        public void TautRope_StaysNearlyStraight()
        {
            Run(new Vector3(0f, 2f, 0f), new Vector3(4f, 2f, 0f), 4f, new BoxObstacles());

            foreach (Vector3 point in _loose.Positions)
                Assert.InRange(point.Y, 2f - RopeSag.TautFreedom - 0.001f, 2f + 0.001f);
        }

        [Fact]
        public void MovingEnd_MakesSlackRopeLagBehind()
        {
            Vector3 start = new Vector3(0f, 2f, 0f);
            Run(start, new Vector3(4f, 2f, 0f), 5f, new BoxObstacles());
            Vector3 moved = new Vector3(4f, 2f, 1f);
            Step(start, moved, 5f, new BoxObstacles());
            int middle = _targets.Count / 2;

            Assert.True(Vector3.Distance(_loose.Positions[middle], _targets[middle]) > 0.01f);
        }

        private void Run(Vector3 start, Vector3 end, float ropeLength, BoxObstacles obstacles)
        {
            for (int i = 0; i < Frames; i++)
                Step(start, end, ropeLength, obstacles);
        }

        private void Step(Vector3 start, Vector3 end, float ropeLength, BoxObstacles obstacles)
        {
            _path.Update(start, end, new RopeLengthState(ropeLength, 0f, 0f), Radius, DeltaTime, obstacles);
            _sag.Build(_path, Down, Bones, Radius, obstacles, _targets, _freedoms, _spacings);
            _loose.Step(_targets, _freedoms, _spacings, Gravity, DeltaTime, Radius, obstacles);
        }
    }
}
