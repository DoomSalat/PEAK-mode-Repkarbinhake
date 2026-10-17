using Repkarbinhake.Core.Interaction;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class HoldActionTests
    {
        private const int Target = 7;
        private const float Duration = 1f;

        private readonly HoldAction _hold = new HoldAction();

        [Fact]
        public void Inactive_NeverCompletes()
        {
            Assert.False(_hold.Advance(true, Target, 10f));
            Assert.Equal(0f, _hold.Progress);
        }

        [Fact]
        public void HoldingLongEnough_CompletesOnceAndResets()
        {
            _hold.Begin(Target, Duration);

            Assert.False(_hold.Advance(true, Target, 0.6f));
            Assert.True(_hold.Advance(true, Target, 0.6f));
            Assert.False(_hold.IsActive);
            Assert.False(_hold.Advance(true, Target, 5f));
        }

        [Fact]
        public void Progress_FillsTowardOne()
        {
            _hold.Begin(Target, Duration);

            _hold.Advance(true, Target, 0.25f);

            Assert.Equal(0.25f, _hold.Progress, 4);
        }

        [Fact]
        public void ReleasingKey_Cancels()
        {
            _hold.Begin(Target, Duration);
            _hold.Advance(true, Target, 0.9f);

            Assert.False(_hold.Advance(false, Target, 0.2f));
            Assert.False(_hold.IsActive);
            Assert.Equal(0f, _hold.Progress);
        }

        [Fact]
        public void LookingAway_Cancels()
        {
            _hold.Begin(Target, Duration);
            _hold.Advance(true, Target, 0.9f);

            Assert.False(_hold.Advance(true, HoldAction.NoTarget, 0.2f));
            Assert.False(_hold.IsActive);
        }

        [Fact]
        public void SwitchingToAnotherPlayer_Cancels()
        {
            _hold.Begin(Target, Duration);

            Assert.False(_hold.Advance(true, Target + 1, 2f));
            Assert.False(_hold.IsActive);
        }
    }
}
