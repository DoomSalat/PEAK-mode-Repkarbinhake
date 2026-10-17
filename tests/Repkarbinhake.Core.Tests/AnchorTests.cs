using System.Numerics;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class AnchorTests
    {
        private const float MinLength = 1.5f;
        private const float MaxLength = 14f;
        private const int Precision = 3;

        private readonly AnchorLength _length = new AnchorLength();
        private readonly AnchorPayloadCodec _codec = new AnchorPayloadCodec();

        [Fact]
        public void AnchorActor_RoundTripsToItsOwner()
        {
            int anchor = AnchorActors.Of(7);

            Assert.True(AnchorActors.IsAnchor(anchor));
            Assert.Equal(7, AnchorActors.OwnerOf(anchor));
            Assert.False(AnchorActors.IsAnchor(7));
        }

        [Fact]
        public void NumbersBeyondOwnerRange_AreNotAnchors()
        {
            Assert.True(AnchorActors.IsAnchor(-AnchorActors.MaxOwner));
            Assert.False(AnchorActors.IsAnchor(-AnchorActors.MaxOwner - 1));
            Assert.False(AnchorActors.IsAnchor(-1000000));
        }

        [Fact]
        public void Supported_RopePaysOutWithTheClimber()
        {
            _length.Next(3f, true, false, MinLength, MaxLength);

            Assert.Equal(5f + AnchorLength.Slack, _length.Next(5f, true, false, MinLength, MaxLength), Precision);
        }

        [Fact]
        public void PayOut_StopsAtMaxLength()
        {
            Assert.Equal(MaxLength, _length.Next(20f, true, false, MinLength, MaxLength), Precision);
        }

        [Fact]
        public void Falling_LocksTheLength()
        {
            _length.Next(4f, true, false, MinLength, MaxLength);
            float locked = _length.Next(6f, false, true, MinLength, MaxLength);

            Assert.True(_length.IsLocked);
            Assert.Equal(4f + AnchorLength.Slack, locked, Precision);
            Assert.Equal(locked, _length.Next(9f, false, true, MinLength, MaxLength), Precision);
        }

        [Fact]
        public void HangingWithoutSupport_StaysLocked()
        {
            _length.Next(4f, true, false, MinLength, MaxLength);
            _length.Next(5f, false, true, MinLength, MaxLength);

            Assert.Equal(4f + AnchorLength.Slack, _length.Next(5f, false, false, MinLength, MaxLength), Precision);
        }

        [Fact]
        public void BackOnSupport_Unlocks()
        {
            _length.Next(4f, true, false, MinLength, MaxLength);
            _length.Next(5f, false, true, MinLength, MaxLength);
            _length.Next(5f, true, false, MinLength, MaxLength);

            Assert.False(_length.IsLocked);
        }

        [Fact]
        public void Codec_RoundTripsPlacement()
        {
            AnchorPlacement placement = new AnchorPlacement(new Vector3(1.25f, -3.5f, 100.125f), Vector3.UnitZ, Vector3.UnitY);

            Assert.True(_codec.TryDecode(_codec.Encode(placement), out AnchorPlacement decoded));
            Assert.Equal(placement.Position, decoded.Position);
            Assert.Equal(placement.Forward, decoded.Forward);
            Assert.Equal(placement.Up, decoded.Up);
        }

        [Fact]
        public void Codec_ParsesOnlyAnchorKeys()
        {
            Assert.True(_codec.TryParseKey(_codec.KeyOf(5), out int owner));
            Assert.Equal(5, owner);
            Assert.False(_codec.TryParseKey("rkb.link.1_2", out _));
            Assert.False(_codec.TryDecode("1|2|3", out _));
        }
    }
}
