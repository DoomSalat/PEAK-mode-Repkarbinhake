using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Tests.Fakes;
using Repkarbinhake.Core.Visual;
using Xunit;
using Xunit.Abstractions;

namespace Repkarbinhake.Core.Tests
{
    public class RopeLipTests
    {
        private const float Radius = 0.05f;
        private const float DeltaTime = 0.02f;
        private const float RopeLength = 5.5f;
        private const int Frames = 30;
        private const float NearCorner = 0.2f;

        private static readonly Vector3 Puller = new Vector3(-3f, 0.8f, 0f); // Static: shared test fixture
        private static readonly Vector3 Hanging = new Vector3(0.4f, -2.5f, 0f); // Static: shared test fixture
        private static readonly Vector3 InwardContact = new Vector3(-0.6f, 0.1f, 0f); // Static: shared test fixture

        private readonly BoxObstacles _ledge = new BoxObstacles().Add(new Vector3(-6f, -6f, -2f), new Vector3(0f, 0f, 2f));
        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly ITestOutputHelper _output;

        public RopeLipTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ContactBehindTheEdge_SlidesOntoTheEdge()
        {
            Vector3 contact = SettleFrom(InwardContact);

            Assert.True(new Vector2(contact.X, contact.Y).Length() < NearCorner, $"contact {contact}");
        }

        [Fact]
        public void HangingPlayer_IsPulledUpTheWallNotIntoIt()
        {
            Vector3 contact = SettleFrom(InwardContact);
            Vector3 pull = Vector3.Normalize(contact - Hanging);

            Assert.True(pull.X > -0.2f, $"pull {pull}");
        }

        private Vector3 SettleFrom(Vector3 contact)
        {
            RopeLengthState length = new RopeLengthState(RopeLength, 0f, 0f);
            _path.Update(Puller, Hanging, length, Radius, DeltaTime, _ledge);
            _path.Follow(new List<Vector3> { contact }, new List<float>());
            _path.Update(Puller, Hanging, length, Radius, DeltaTime, _ledge);
            _path.Lead();

            for (int i = 0; i < Frames; i++)
                _path.Update(Puller, Hanging, length, Radius, DeltaTime, _ledge);

            Vector3 settled = _path.Points[_path.Points.Count - 2];
            _output.WriteLine($"contacts {_path.ContactCount}, lower contact {settled}, points {string.Join(" ", _path.Points)}");
            return settled;
        }
    }
}
