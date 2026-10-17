using System.Numerics;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class LedgeAssistTests
    {
        private const float Push = 10f;
        private const float Step = 0.1f;

        private static readonly Vector3 Center = new Vector3(0f, 0f, 0f); // Static: shared test fixture
        private static readonly Vector3 PullIntoWall = new Vector3(0f, 20f, 20f); // Static: shared test fixture

        private readonly BoxObstacles _wall = new BoxObstacles().Add(new Vector3(-5f, -5f, 0.4f), new Vector3(5f, 5f, 2f));
        private readonly LedgeAssist _assist = new LedgeAssist();

        [Fact]
        public void Stuck_AgainstWall_SlidesAndPushesOff()
        {
            Vector3 force = Tick(PullIntoWall, Vector3.Zero, 4);

            Assert.True(_assist.IsPushing);
            Assert.Equal(-Push, force.Z, 3);
            Assert.Equal(20f, force.Y, 3);
        }

        [Fact]
        public void BeforeStuckTime_KeepsPull()
        {
            Vector3 force = Tick(PullIntoWall, Vector3.Zero, 2);

            Assert.False(_assist.IsPushing);
            Assert.Equal(PullIntoWall, force);
        }

        [Fact]
        public void MovingAlongPull_KeepsPull()
        {
            Vector3 force = Tick(PullIntoWall, new Vector3(0f, 1f, 1f), 10);

            Assert.False(_assist.IsPushing);
            Assert.Equal(PullIntoWall, force);
        }

        [Fact]
        public void NoWallAhead_KeepsPull()
        {
            Vector3 pull = new Vector3(0f, 20f, -20f);
            Vector3 force = Tick(pull, Vector3.Zero, 10);

            Assert.False(_assist.IsPushing);
            Assert.Equal(pull, force);
        }

        [Fact]
        public void CannotAssist_ResetsTimer()
        {
            Tick(PullIntoWall, Vector3.Zero, 10);
            _assist.Adjust(PullIntoWall, Center, Vector3.Zero, false, Push, Step, _wall);

            Assert.Equal(0f, _assist.StuckFor);
            Assert.False(_assist.IsPushing);
        }

        private Vector3 Tick(Vector3 pull, Vector3 velocity, int frames)
        {
            Vector3 force = pull;

            for (int i = 0; i < frames; i++)
                force = _assist.Adjust(pull, Center, velocity, true, Push, Step, _wall);

            return force;
        }
    }
}
