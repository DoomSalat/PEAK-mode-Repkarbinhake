using System.Numerics;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Tests.Fakes;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeWrapPathTests
    {
        private const float Radius = 0.05f;
        private const float DeltaTime = 0.02f;
        private const float RopeLength = 6f;
        private const int Settle = 30;
        private const int Steps = 60;
        private const int FlowFrames = 100;

        private static readonly Vector3 Start = new Vector3(0f, 0.5f, 0f); // Static: shared constant for all tests
        private static readonly Vector3 End = new Vector3(4f, 0.5f, 0f); // Static: shared constant for all tests

        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly BoxObstacles _wall = new BoxObstacles().Add(new Vector3(1.5f, -5f, -5f), new Vector3(2.5f, 1f, 5f));
        private RopeLengthState _length = new RopeLengthState(RopeLength, 0f, 0f);

        [Fact]
        public void ClearLine_StaysStraight()
        {
            Run(Start, End, new BoxObstacles());

            Assert.Equal(0, _path.ContactCount);
            Assert.Equal(4f, _path.PathLength(), 3);
        }

        [Fact]
        public void WallBetweenEnds_RopeWrapsOverIt()
        {
            LowerOverWall();

            Assert.True(_path.ContactCount > 0, Describe());
            Assert.True(_path.PathLength() > 4.1f, Describe());
            AssertSegmentsClear(_wall);
        }

        [Fact]
        public void WrappedRope_HugsTheWallTop()
        {
            LowerOverWall();

            foreach (Vector3 point in _path.Points)
                Assert.True(point.Y < 1f + Radius * RopeWrapPath.ContactLift + 0.1f);
        }

        [Fact]
        public void EndsLiftedAboveWall_ContactsAreRemoved()
        {
            LowerOverWall();
            Run(new Vector3(0f, 3f, 0f), new Vector3(4f, 3f, 0f), _wall);

            Assert.Equal(0, _path.ContactCount);
            Assert.Equal(RopeLength, _path.Materials[0], 3);
        }

        [Fact]
        public void SettledWrap_DoesNotJitter()
        {
            LowerOverWall();
            int contacts = _path.ContactCount;
            float length = _path.PathLength();
            Run(Start, End, _wall);

            Assert.Equal(contacts, _path.ContactCount);
            Assert.Equal(length, _path.PathLength(), 2);
        }

        [Fact]
        public void Span_FollowsPathToFirstContact()
        {
            LowerOverWall();

            Assert.True(_path.TrySpan(true, out RopeSpan span));
            Assert.True(span.Direction.Y > 0f);
            Assert.Equal(_path.PathLength(), span.PathLength, 3);
        }

        [Fact]
        public void ManyObstacles_ContactsAreCapped()
        {
            BoxObstacles teeth = new BoxObstacles();

            for (int i = 0; i < 60; i++)
                teeth.Add(new Vector3(0.2f + i * 0.13f, -5f, -5f), new Vector3(0.25f + i * 0.13f, 1f + (i % 2) * 0.4f, 5f));

            Run(Start, new Vector3(8.5f, 0.5f, 0f), teeth);

            Assert.True(_path.ContactCount <= RopeWrapPath.MaxContacts);
        }

        [Fact]
        public void WrapAndUnwrap_KeepTotalMaterial()
        {
            LowerOverWall();

            Assert.Equal(RopeLength, Sum(), 3);
        }

        [Fact]
        public void FirstEndReeling_ShortensOnlyTheFarSegment()
        {
            TautOverWall();
            Vector3[] contacts = Contacts();
            float[] before = Materials();
            _length = new RopeLengthState(_length.Total - 0.5f, _length.FirstReel + 0.5f, 0f);
            Run(Start, End, _wall);

            int last = _path.Materials.Count - 1;
            Assert.Equal(before[last] - 0.5f, _path.Materials[last], 3);

            for (int i = 0; i < last; i++)
                Assert.Equal(before[i], _path.Materials[i], 3);

            Assert.Equal(contacts, Contacts());
        }

        [Fact]
        public void SecondEndReeling_ShortensOnlyTheFirstSegment()
        {
            TautOverWall();
            float[] before = Materials();
            _length = new RopeLengthState(_length.Total - 0.5f, 0f, _length.SecondReel + 0.5f);
            Run(Start, End, _wall);

            Assert.Equal(before[0] - 0.5f, _path.Materials[0], 3);
        }

        [Fact]
        public void SlackSide_FeedsRopeThroughTheContact()
        {
            LowerOverWall();
            _length = new RopeLengthState(RopeLength - 1.8f, 1.8f, 0f);

            for (int i = 0; i < FlowFrames; i++)
                _path.Update(Start, End, _length, Radius, DeltaTime, _wall);

            int last = _path.Materials.Count - 1;
            Assert.True(_path.Materials[last] > _path.Chord(last) * 0.9f, Describe());
        }

        private void TautOverWall()
        {
            _length = new RopeLengthState(4.1f, 0f, 0f);
            LowerOverWall();
        }

        private void LowerOverWall()
        {
            for (int i = 0; i <= Steps; i++)
            {
                float height = 3f + (Start.Y - 3f) * i / Steps;
                _path.Update(new Vector3(Start.X, height, 0f), new Vector3(End.X, height, 0f), _length, Radius, DeltaTime, _wall);
            }

            Run(Start, End, _wall);
        }

        private void Run(Vector3 start, Vector3 end, BoxObstacles obstacles)
        {
            for (int i = 0; i < Settle; i++)
                _path.Update(start, end, _length, Radius, DeltaTime, obstacles);
        }

        private float Sum()
        {
            float total = 0f;

            foreach (float material in _path.Materials)
                total += material;

            return total;
        }

        private float[] Materials()
        {
            float[] materials = new float[_path.Materials.Count];

            for (int i = 0; i < materials.Length; i++)
                materials[i] = _path.Materials[i];

            return materials;
        }

        private Vector3[] Contacts()
        {
            Vector3[] contacts = new Vector3[_path.ContactCount];

            for (int i = 0; i < contacts.Length; i++)
                contacts[i] = _path.Points[i + 1];

            return contacts;
        }

        private string Describe()
        {
            return string.Join(" ", System.Linq.Enumerable.Select(_path.Points, p => $"({p.X:0.00},{p.Y:0.00})")) + " | " + string.Join(" ", System.Linq.Enumerable.Select(_path.Materials, m => $"{m:0.00}"));
        }

        private void AssertSegmentsClear(BoxObstacles obstacles)
        {
            for (int i = 1; i < _path.Points.Count; i++)
                Assert.False(obstacles.IsBlocked(_path.Points[i - 1], _path.Points[i], Radius), Describe());
        }
    }
}
