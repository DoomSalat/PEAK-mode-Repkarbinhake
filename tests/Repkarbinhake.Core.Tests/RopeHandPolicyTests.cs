using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeHandPolicyTests
    {
        private readonly RopeHandPolicy _policy = new RopeHandPolicy();

        [Theory]
        [InlineData(Footing.Grounded, true, true, true, true)]
        [InlineData(Footing.Airborne, true, false, false, true)]
        [InlineData(Footing.Climbing, false, false, false, false)]
        [InlineData(Footing.Unconscious, false, false, false, false)]
        public void Rules_MatchFootingTable(Footing footing, bool hold, bool brace, bool reelIn, bool reelOut)
        {
            Assert.Equal(hold, _policy.CanHold(footing));
            Assert.Equal(brace, _policy.CanBrace(footing));
            Assert.Equal(reelIn, _policy.CanReelIn(footing));
            Assert.Equal(reelOut, _policy.CanReelOut(footing));
        }
    }
}
