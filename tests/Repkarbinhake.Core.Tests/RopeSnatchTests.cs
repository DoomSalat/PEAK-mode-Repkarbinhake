using Repkarbinhake.Core.Physics;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeSnatchTests
    {
        private const string Key = "rkb.link.1_2";
        private const float DeltaTime = 0.02f;
        private const float Precision = 0.001f;

        private readonly RopeSnatch _snatch = new RopeSnatch();

        [Fact]
        public void FirstMeasure_IsZero()
        {
            Assert.Equal(0f, _snatch.Measure(Key, 0.5f, 0f, DeltaTime));
        }

        [Fact]
        public void FastGrowingStretch_GivesItsSpeed()
        {
            _snatch.Measure(Key, 0f, 0f, DeltaTime);

            Assert.Equal(10f, _snatch.Measure(Key, 0.2f, 0f, DeltaTime), 3);
        }

        [Fact]
        public void SlackRope_NeverSnatches()
        {
            _snatch.Measure(Key, -2f, 0f, DeltaTime);

            Assert.Equal(0f, _snatch.Measure(Key, -1f, 0f, DeltaTime));
        }

        [Fact]
        public void TighteningFromSlack_CountsOnlyTheTautPart()
        {
            _snatch.Measure(Key, -1f, 0f, DeltaTime);

            Assert.Equal(5f, _snatch.Measure(Key, 0.1f, 0f, DeltaTime), 3);
        }

        [Fact]
        public void OwnMovementAway_IsNotBlamedOnThePartner()
        {
            _snatch.Measure(Key, 0f, 0f, DeltaTime);

            Assert.Equal(2f, _snatch.Measure(Key, 0.2f, 8f, DeltaTime), 3);
        }

        [Fact]
        public void LooseningRope_IsZero()
        {
            _snatch.Measure(Key, 0.5f, 0f, DeltaTime);

            Assert.Equal(0f, _snatch.Measure(Key, 0.3f, 0f, DeltaTime), Precision);
        }

        [Fact]
        public void TeleportJump_IsIgnored()
        {
            _snatch.Measure(Key, 0f, 0f, DeltaTime);

            Assert.Equal(0f, _snatch.Measure(Key, RopeSnatch.MaxStepGrowth + 1f, 0f, DeltaTime));
        }

        [Fact]
        public void ForgottenLink_StartsOver()
        {
            _snatch.Measure(Key, 0f, 0f, DeltaTime);
            _snatch.Forget(Key);

            Assert.Equal(0f, _snatch.Measure(Key, 0.2f, 0f, DeltaTime));
        }
    }
}
