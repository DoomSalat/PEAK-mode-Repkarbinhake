using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopePathSyncCoreTests
    {
        private const int Low = 2;
        private const int High = 5;
        private const int Dummy = -2;

        private readonly RopePathAuthority _authority = new RopePathAuthority();
        private readonly RopePathCodec _codec = new RopePathCodec();

        [Fact]
        public void NobodyHolds_LowerActorOwns()
        {
            RopeLink link = new RopeLink(High, Low, 1);

            Assert.Equal(Low, _authority.OwnerOf(link));
        }

        [Fact]
        public void OneHolds_HolderOwns()
        {
            RopeLink link = new RopeLink(Low, High, 1);
            link.Second.IsHeld = true;

            Assert.Equal(High, _authority.OwnerOf(link));
            Assert.True(_authority.Owns(link, High));
            Assert.False(_authority.Owns(link, Low));
        }

        [Fact]
        public void BothHold_LowerActorOwns()
        {
            RopeLink link = new RopeLink(Low, High, 1);
            link.First.IsHeld = true;
            link.Second.IsHeld = true;

            Assert.Equal(Low, _authority.OwnerOf(link));
        }

        [Fact]
        public void LocalOnlyRope_IsAlwaysOwnedLocally()
        {
            RopeLink link = new RopeLink(Dummy, Low, 1);

            Assert.False(_authority.IsShared(link));
            Assert.True(_authority.Owns(link, Low));
        }

        [Fact]
        public void Codec_RoundTripsContactsAndMaterials()
        {
            Vector3[] points = { Vector3.Zero, new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f), new Vector3(9f, 0f, 0f) };
            float[] materials = { 1.5f, 2f, 3.25f };
            List<Vector3> contacts = new List<Vector3>();
            List<float> decoded = new List<float>();

            Assert.True(_codec.TryDecode(_codec.Encode(points, materials), contacts, decoded));
            Assert.Equal(new[] { points[1], points[2] }, contacts);
            Assert.Equal(materials, decoded);
        }

        [Fact]
        public void Codec_RejectsBrokenData()
        {
            List<Vector3> contacts = new List<Vector3>();
            List<float> materials = new List<float>();

            Assert.False(_codec.TryDecode(new[] { 2f, 1f, 2f }, contacts, materials));
            Assert.False(_codec.TryDecode(null, contacts, materials));
        }
    }
}
