using System.Numerics;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class MantleAssistTests
    {
        private const float Step = 0.8f;
        private const float Push = 30f;
        private const float Tick = 0.1f;

        private static readonly Vector3 Hanging = new Vector3(0f, -0.8f, -0.4f); // Static: shared test fixture
        private static readonly Vector3 Lip = new Vector3(0f, 0.1f, 0f); // Static: shared test fixture
        private static readonly Vector3 Puller = new Vector3(0f, 1f, 6f); // Static: shared test fixture

        private readonly BoxObstacles _ledge = new BoxObstacles().Add(new Vector3(-5f, -5f, 0f), new Vector3(5f, 0f, 10f));
        private readonly MantleAssist _assist = new MantleAssist();

        [Fact]
        public void StuckUnderLip_PushesTowardFootingPastTheEdge()
        {
            Vector3 force = Run(Hanging, Vector3.Zero, true, 5);

            Assert.True(_assist.IsPushing);
            Assert.Equal(Step, _assist.Target.Z, 1);
            Assert.True(force.Y > 0f && force.Z > 0f);
            Assert.Equal(Push, force.Length(), 3);
        }

        [Fact]
        public void BeforeStuckTime_DoesNothing()
        {
            Vector3 force = Run(Hanging, Vector3.Zero, true, 2);

            Assert.False(_assist.IsPushing);
            Assert.Equal(Vector3.Zero, force);
        }

        [Fact]
        public void MovingFast_DoesNothing()
        {
            Vector3 force = Run(Hanging, new Vector3(0f, 2f, 0f), true, 10);

            Assert.False(_assist.IsPushing);
            Assert.Equal(Vector3.Zero, force);
        }

        [Fact]
        public void FarBelowTheLip_DoesNothing()
        {
            Vector3 force = Run(new Vector3(0f, -3f, -0.4f), Vector3.Zero, true, 10);

            Assert.False(_assist.IsPushing);
            Assert.Equal(Vector3.Zero, force);
        }

        [Fact]
        public void NoFloorPastTheEdge_DoesNothing()
        {
            MantleAssist assist = new MantleAssist();
            BoxObstacles thinLedge = new BoxObstacles().Add(new Vector3(-5f, -5f, 0f), new Vector3(5f, 0f, 0.3f));
            Vector3 force = Vector3.Zero;

            for (int i = 0; i < 10; i++)
                force = assist.Push(Hanging, Vector3.Zero, Lip, Puller, true, Step, Push, Tick, thinLedge);

            Assert.False(assist.IsPushing);
            Assert.Equal(Vector3.Zero, force);
        }

        [Fact]
        public void CannotAssist_StopsPushing()
        {
            Run(Hanging, Vector3.Zero, true, 5);
            Vector3 force = _assist.Push(Hanging, Vector3.Zero, Lip, Puller, false, Step, Push, Tick, _ledge);

            Assert.False(_assist.IsPushing);
            Assert.Equal(Vector3.Zero, force);
        }

        private Vector3 Run(Vector3 center, Vector3 velocity, bool canAssist, int frames)
        {
            Vector3 force = Vector3.Zero;

            for (int i = 0; i < frames; i++)
                force = _assist.Push(center, velocity, Lip, Puller, canAssist, Step, Push, Tick, _ledge);

            return force;
        }
    }
}
