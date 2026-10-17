using System.Linq;
using System.Numerics;
using Repkarbinhake.Core.Tests.Fakes;
using Repkarbinhake.Core.Visual;
using Xunit;
using Xunit.Abstractions;

namespace Repkarbinhake.Core.Tests
{
    public class RopeClimbTests
    {
        private const float Radius = 0.05f;
        private const float DeltaTime = 0.02f;
        private const float Height = 1f;
        private const float EndHeight = 0.3f;
        private const float Side = 3f;
        private const int SweepFrames = 60;
        private const int SettleFrames = 200;
        private const float StuckHeight = 0.45f;

        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly ITestOutputHelper _output;

        public RopeClimbTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GentleRock_RopeRidesUpAndOver()
        {
            float before = SweepInto(new PyramidObstacles(Height, 1f), out float after);

            Assert.True(after > StuckHeight + 0.1f, $"before {before:0.00}, after {after:0.00}");
        }

        [Fact]
        public void SteepRock_HoldsTheRope()
        {
            float before = SweepInto(new PyramidObstacles(Height, 5f), out float after);

            Assert.True(after < before + 0.05f, $"before {before:0.00}, after {after:0.00}");
        }

        private float SweepInto(PyramidObstacles rock, out float after)
        {
            float length = 2f * Side;

            for (int i = 0; i <= SweepFrames; i++)
            {
                float z = 1.5f - 1.5f * i / SweepFrames;
                Step(new Vector3(-Side, EndHeight, z), new Vector3(Side, EndHeight, z), length, rock);
            }

            float before = HighestContact();

            for (int i = 0; i < SettleFrames; i++)
                Step(new Vector3(-Side, EndHeight, 0f), new Vector3(Side, EndHeight, 0f), length, rock);

            after = HighestContact();
            _output.WriteLine(string.Join(" ", _path.Points.Select(p => $"({p.X:0.00},{p.Y:0.00},{p.Z:0.00})")));
            return before;
        }

        private void Step(Vector3 start, Vector3 end, float length, PyramidObstacles rock)
        {
            _path.Update(start, end, new RopeLengthState(length, 0f, 0f), Radius, DeltaTime, rock);
        }

        private float HighestContact()
        {
            float highest = EndHeight;

            for (int i = 1; i < _path.Points.Count - 1; i++)
                highest = System.MathF.Max(highest, _path.Points[i].Y);

            return highest;
        }
    }
}
