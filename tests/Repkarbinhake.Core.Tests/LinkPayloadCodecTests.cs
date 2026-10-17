using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class LinkPayloadCodecTests
    {
        private readonly LinkPayloadCodec _codec = new LinkPayloadCodec();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Link_RoundTrips(bool tied)
        {
            string key = LinkKeys.Build(3, 7);
            LinkUpdate original = new LinkUpdate(key, new LinkState(tied, LinkState.ComposeTieId(7, 42)), 7, 42);

            Assert.True(_codec.TryDecodeLink(key, _codec.EncodeLink(original), out LinkUpdate decoded));
            Assert.Equal(original.Key, decoded.Key);
            Assert.Equal(original.State.IsTied, decoded.State.IsTied);
            Assert.Equal(original.State.TieId, decoded.State.TieId);
            Assert.Equal(original.WriterActor, decoded.WriterActor);
            Assert.Equal(original.Sequence, decoded.Sequence);
        }

        [Theory]
        [InlineData(true, 3.25f)]
        [InlineData(false, 0f)]
        [InlineData(true, 0.1f)]
        public void End_RoundTripsExactly(bool held, float reel)
        {
            string key = LinkKeys.BuildEnd(3, 7, 3);
            EndState original = new EndState(LinkState.ComposeTieId(7, 42), held, reel);

            Assert.True(_codec.TryDecodeEnd(key, _codec.EncodeEnd(original), out EndUpdate decoded));
            Assert.Equal(original.TieId, decoded.State.TieId);
            Assert.Equal(original.IsHeld, decoded.State.IsHeld);
            Assert.Equal(original.Reel, decoded.State.Reel);
        }

        [Fact]
        public void SameState_EncodesToSameString_ForCompareAndSwap()
        {
            LinkUpdate update = new LinkUpdate(LinkKeys.Build(1, 2), LinkState.Untied, 1, 9);

            Assert.Equal(_codec.EncodeLink(update), _codec.EncodeLink(update));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("garbage")]
        [InlineData("2|1|5|1|1")]
        [InlineData("1|1|x|1|1")]
        [InlineData("1|1|5|1")]
        public void BrokenLinkPayload_IsRejected(string payload)
        {
            Assert.False(_codec.TryDecodeLink(LinkKeys.Build(1, 2), payload, out _));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("1|5|1")]
        [InlineData("1|5|1|abc")]
        [InlineData("9|5|1|1.0")]
        public void BrokenEndPayload_IsRejected(string payload)
        {
            Assert.False(_codec.TryDecodeEnd(LinkKeys.BuildEnd(1, 2, 1), payload, out _));
        }
    }
}
