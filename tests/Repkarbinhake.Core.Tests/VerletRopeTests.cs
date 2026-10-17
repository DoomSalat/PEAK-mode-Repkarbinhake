using System;
using System.Numerics;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class VerletRopeTests
    {
        private const float DeltaTime = 0.02f;
        private const float Radius = 0.05f;

        private static readonly Vector3 Gravity = new Vector3(0f, -9.81f, 0f); // Static: shared constant for all tests
        private static readonly Vector3 Start = new Vector3(0f, 5f, 0f); // Static: shared constant for all tests
        private static readonly Vector3 End = new Vector3(4f, 5f, 0f); // Static: shared constant for all tests

        private readonly VerletRope _rope = new VerletRope();

        [Fact]
        public void Resize_FromEmpty_FillsStraightLineBetweenEnds()
        {
            _rope.Resize(5, Start, End);

            Assert.Equal(5, _rope.Count);
            Assert.Equal(Start, _rope.Positions[0]);
            Assert.Equal(End, _rope.Positions[4]);
            Assert.Equal(new Vector3(2f, 5f, 0f), _rope.Positions[2]);
        }

        [Fact]
        public void Resize_KeepsEndsAndShape()
        {
            _rope.Resize(5, Start, End);
            Simulate(6f, 100);
            float sagBefore = LowestY();

            _rope.Resize(17, Start, End);

            Assert.Equal(17, _rope.Count);
            Assert.Equal(Start, _rope.Positions[0]);
            Assert.Equal(End, _rope.Positions[16]);
            Assert.InRange(LowestY(), sagBefore - 0.05f, sagBefore + 0.3f);
        }

        [Fact]
        public void Step_PinsBothEnds()
        {
            _rope.Resize(10, Start, End);
            Vector3 movedEnd = new Vector3(3f, 2f, 1f);

            _rope.Step(Start, movedEnd, 6f, Gravity, DeltaTime, Radius, null);

            Assert.Equal(Start, _rope.Positions[0]);
            Assert.Equal(movedEnd, _rope.Positions[9]);
        }

        [Fact]
        public void SlackRope_SagsUnderGravity()
        {
            _rope.Resize(12, Start, End);

            Simulate(6f, 200);

            Assert.True(LowestY() < Start.Y - 1f);
        }

        [Fact]
        public void SlackRope_DoesNotStretchBeyondItsLength()
        {
            _rope.Resize(12, Start, End);

            Simulate(6f, 300);

            Assert.True(PolylineLength() <= 6f * 1.03f);
        }

        [Fact]
        public void TautRope_StaysCloseToStraightLine()
        {
            _rope.Resize(12, Start, End);

            Simulate(4f, 300);

            Assert.True(LowestY() > Start.Y - 0.3f);
        }

        [Fact]
        public void Collider_KeepsPointsAboveFloor()
        {
            Vector3 lowStart = new Vector3(0f, 0.5f, 0f);
            Vector3 lowEnd = new Vector3(4f, 0.5f, 0f);
            _rope.Resize(12, lowStart, lowEnd);

            for (int i = 0; i < 300; i++)
                _rope.Step(lowStart, lowEnd, 8f, Gravity, DeltaTime, Radius, new FloorCollider());

            Assert.True(LowestY() >= Radius - 0.001f);
        }

        [Fact]
        public void PointsTunneledIntoShell_ArePulledBackAlongTheChain()
        {
            Vector3 lowStart = new Vector3(0f, 0.5f, 0f);
            Vector3 lowEnd = new Vector3(4f, 0.5f, 0f);
            _rope.Resize(12, lowStart, lowEnd);

            for (int i = 0; i < 100; i++)
                _rope.Step(lowStart, lowEnd, 8f, Gravity, DeltaTime, Radius, null);

            Assert.True(LowestY() < 0f);

            _rope.Step(lowStart, lowEnd, 8f, Gravity, DeltaTime, Radius, new ShellFloorCollider());

            Assert.True(LowestY() >= Radius - 0.001f);
        }

        [Fact]
        public void Span_OverBump_PointsUpAndIsLongerThanStraightLine()
        {
            Vector3 left = new Vector3(0f, 0f, 0f);
            Vector3 right = new Vector3(4f, 0f, 0f);
            _rope.Resize(16, left, right);

            for (int i = 0; i < 200; i++)
                _rope.Step(left, right, 3f, Vector3.Zero, DeltaTime, Radius, new BumpCollider());

            Assert.True(_rope.TrySpan(true, 0.5f, out RopeSpan fromLeft));
            Assert.True(_rope.TrySpan(false, 0.5f, out RopeSpan fromRight));
            Assert.True(fromLeft.Direction.Y > 0.3f);
            Assert.True(fromRight.Direction.Y > 0.3f);
            Assert.True(fromRight.Direction.X < 0f);
            Assert.True(fromLeft.PathLength > 4f);
        }

        [Fact]
        public void TautRopeOverLedgeCorner_WrapsInsteadOfCuttingThrough()
        {
            Vector3 holder = new Vector3(0.5f, 2.5f, 0f);
            Vector3 hanger = new Vector3(2.5f, 0f, 0f);
            _rope.Resize(24, holder, hanger);

            for (int i = 0; i < 300; i++)
                _rope.Step(holder, hanger, 3f, Gravity, DeltaTime, Radius, new LedgeCollider());

            Assert.True(_rope.TrySpan(false, 0.5f, out RopeSpan fromHanger));
            Assert.True(fromHanger.Direction.Y > 0.8f);
            Assert.True(fromHanger.PathLength > Vector3.Distance(holder, hanger) + 0.3f);

            foreach (Vector3 point in _rope.Positions)
                Assert.False(point.X < 1.95f && point.Y < 1.95f);
        }

        [Fact]
        public void Span_BeforeFirstStep_IsUnavailable()
        {
            Assert.False(_rope.TrySpan(true, 0.5f, out _));
        }

        private void Simulate(float length, int steps)
        {
            for (int i = 0; i < steps; i++)
                _rope.Step(Start, End, length, Gravity, DeltaTime, Radius, null);
        }

        private float LowestY()
        {
            float lowest = float.MaxValue;

            foreach (Vector3 point in _rope.Positions)
                lowest = MathF.Min(lowest, point.Y);

            return lowest;
        }

        private float PolylineLength()
        {
            float total = 0f;

            for (int i = 1; i < _rope.Count; i++)
                total += Vector3.Distance(_rope.Positions[i - 1], _rope.Positions[i]);

            return total;
        }

        private sealed class LedgeCollider : IRopeCollider
        {
            private const float Edge = 2f;

            public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
            {
                if (to.X >= Edge || to.Y >= Edge)
                    return to;

                return from.X >= Edge ? new Vector3(Edge, to.Y, to.Z) : new Vector3(to.X, Edge, to.Z);
            }
        }

        private sealed class BumpCollider : IRopeCollider
        {
            public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
            {
                bool insideBump = to.X > 1f && to.X < 3f && to.Y < 1f;
                return insideBump ? new Vector3(to.X, 1f, to.Z) : to;
            }
        }

        private sealed class ShellFloorCollider : IRopeCollider
        {
            public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
            {
                bool crossesSurface = from.Y >= radius && to.Y < radius;
                return crossesSurface ? new Vector3(to.X, radius, to.Z) : to;
            }
        }

        private sealed class FloorCollider : IRopeCollider
        {
            public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
            {
                return to.Y < radius ? new Vector3(to.X, radius, to.Z) : to;
            }
        }
    }
}
