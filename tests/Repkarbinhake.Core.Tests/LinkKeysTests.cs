using Repkarbinhake.Core.Data;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class LinkKeysTests
    {
        [Fact]
        public void Build_IsIndependentOfActorOrder()
        {
            Assert.Equal(LinkKeys.Build(3, 7), LinkKeys.Build(7, 3));
        }

        [Fact]
        public void TryParse_RoundTripsBuiltKey()
        {
            bool parsed = LinkKeys.TryParse(LinkKeys.Build(9, 2), out int first, out int second);

            Assert.True(parsed);
            Assert.Equal(2, first);
            Assert.Equal(9, second);
        }

        [Fact]
        public void EndKey_RoundTripsAndKeepsOwner()
        {
            bool parsed = LinkKeys.TryParseEnd(LinkKeys.BuildEnd(9, 2, 9), out int first, out int second, out int owner);

            Assert.True(parsed);
            Assert.Equal(2, first);
            Assert.Equal(9, second);
            Assert.Equal(9, owner);
        }

        [Fact]
        public void EndKey_DiffersFromLinkKey()
        {
            Assert.False(LinkKeys.TryParse(LinkKeys.BuildEnd(1, 2, 1), out _, out _));
            Assert.False(LinkKeys.TryParseEnd(LinkKeys.Build(1, 2), out _, out _, out _));
        }

        [Theory]
        [InlineData("rkb.end.1_2.3")]
        [InlineData("rkb.end.1_2")]
        [InlineData("rkb.end.1_2.x")]
        [InlineData("rkb.end.1_1.1")]
        public void TryParseEnd_RejectsBrokenKeysAndForeignOwner(string key)
        {
            Assert.False(LinkKeys.TryParseEnd(key, out _, out _, out _));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("someOtherMod.key")]
        [InlineData("rkb.link.5")]
        [InlineData("rkb.link.a_b")]
        [InlineData("rkb.link.4_4")]
        [InlineData("rkb.link.1_2_3")]
        public void TryParse_RejectsForeignOrBrokenKeys(string key)
        {
            Assert.False(LinkKeys.TryParse(key, out _, out _));
        }

        [Fact]
        public void PairId_IgnoresOrderAndSeparatesPairs()
        {
            Assert.Equal(LinkKeys.PairId(3, 7), LinkKeys.PairId(7, 3));
            Assert.NotEqual(LinkKeys.PairId(3, 7), LinkKeys.PairId(3, 8));
            Assert.NotEqual(LinkKeys.PairId(-5, 2), LinkKeys.PairId(-2, 5));
        }
    }
}
