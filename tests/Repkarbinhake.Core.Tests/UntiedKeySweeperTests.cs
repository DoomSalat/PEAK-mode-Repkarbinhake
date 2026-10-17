using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class UntiedKeySweeperTests
    {
        private const float MaxAge = 10f;

        private readonly UntiedKeySweeper _sweeper = new UntiedKeySweeper();
        private readonly List<LinkUpdate> _collected = new List<LinkUpdate>();

        [Fact]
        public void TiedUpdate_IsNotTracked()
        {
            _sweeper.Observe(Update(1, 2, tied: true, sequence: 1), 0f);

            Assert.Equal(0, _sweeper.TrackedCount);
        }

        [Fact]
        public void FreshUntie_IsNotCollected()
        {
            _sweeper.Observe(Update(1, 2, tied: false, sequence: 1), 0f);

            _sweeper.Collect(5f, MaxAge, _collected);

            Assert.Empty(_collected);
            Assert.Equal(1, _sweeper.TrackedCount);
        }

        [Fact]
        public void OldUntie_IsCollectedOnceWithExactValueForCompareAndSwap()
        {
            LinkUpdate untie = Update(1, 2, tied: false, sequence: 7);
            _sweeper.Observe(untie, 0f);

            _sweeper.Collect(MaxAge, MaxAge, _collected);

            LinkUpdate collected = Assert.Single(_collected);
            Assert.Equal(untie.Sequence, collected.Sequence);
            Assert.Equal(untie.WriterActor, collected.WriterActor);

            _sweeper.Collect(MaxAge * 2f, MaxAge, _collected);
            Assert.Empty(_collected);
        }

        [Fact]
        public void RetieBeforeExpiry_CancelsSweep()
        {
            _sweeper.Observe(Update(1, 2, tied: false, sequence: 1), 0f);
            _sweeper.Observe(Update(1, 2, tied: true, sequence: 2), 3f);

            _sweeper.Collect(100f, MaxAge, _collected);

            Assert.Empty(_collected);
        }

        [Fact]
        public void RepeatedUntie_RestartsAgeAndKeepsLatestValue()
        {
            _sweeper.Observe(Update(1, 2, tied: false, sequence: 1), 0f);
            _sweeper.Observe(Update(1, 2, tied: false, sequence: 5), 8f);

            _sweeper.Collect(12f, MaxAge, _collected);
            Assert.Empty(_collected);

            _sweeper.Collect(18f, MaxAge, _collected);
            Assert.Equal(5, Assert.Single(_collected).Sequence);
        }

        [Fact]
        public void Forget_DropsKey()
        {
            LinkUpdate untie = Update(1, 2, tied: false, sequence: 1);
            _sweeper.Observe(untie, 0f);

            _sweeper.Forget(untie.Key);

            Assert.Equal(0, _sweeper.TrackedCount);
        }

        private LinkUpdate Update(int first, int second, bool tied, int sequence)
        {
            LinkState state = tied ? new LinkState(true, 99L) : LinkState.Untied;
            return new LinkUpdate(LinkKeys.Build(first, second), state, first, sequence);
        }
    }
}
