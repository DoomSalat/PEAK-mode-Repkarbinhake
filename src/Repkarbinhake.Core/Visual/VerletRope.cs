using System;
using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Physics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class VerletRope
    {
        private const float Damping = 0.98f;
        private const int Iterations = 12;
        private const float SharedCorrection = 0.5f;
        private const int CollisionInterval = 3;

        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Vector3> _snapshot = new List<Vector3>();
        private readonly List<Vector3> _previous = new List<Vector3>();
        private readonly List<Vector3> _resampled = new List<Vector3>();

        public IReadOnlyList<Vector3> Positions => _positions;
        public int Count => _positions.Count;

        public void Resize(int count, Vector3 start, Vector3 end)
        {
            if (count == _positions.Count)
                return;

            if (_positions.Count < 2)
            {
                FillStraight(count, start, end);
                return;
            }

            Resample(count);
        }

        public void Step(Vector3 start, Vector3 end, float length, Vector3 gravity, float deltaTime, float radius, IRopeCollider collider)
        {
            int last = _positions.Count - 1;
            Vector3 gravityStep = gravity * deltaTime * deltaTime;

            for (int i = 1; i < last; i++)
            {
                Vector3 current = _positions[i];
                Vector3 next = current + (current - _previous[i]) * Damping + gravityStep;
                _previous[i] = current;
                _positions[i] = collider == null ? next : collider.Resolve(current, next, radius);
            }

            _positions[0] = start;
            _previous[0] = start;
            _positions[last] = end;
            _previous[last] = end;
            float restLength = length / last;

            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                if (collider != null && iteration % CollisionInterval == 0)
                    TakeSnapshot();

                SatisfyConstraints(restLength);

                if (collider != null && iteration % CollisionInterval == CollisionInterval - 1)
                    CollideFromSnapshot(collider, radius);
            }

            if (collider != null)
                CollideAlongChain(collider, radius);
        }

        public bool TrySpan(bool fromStart, float probeDistance, out RopeSpan span)
        {
            span = default;

            if (_positions.Count < 2)
                return false;

            int last = _positions.Count - 1;
            int origin = fromStart ? 0 : last;
            int step = fromStart ? 1 : -1;
            Vector3 self = _positions[origin];
            Vector3 probe = _positions[origin + step];

            for (int i = origin + step; i >= 0 && i <= last; i += step)
            {
                probe = _positions[i];

                if (Vector3.DistanceSquared(self, probe) >= probeDistance * probeDistance)
                    break;
            }

            Vector3 offset = probe - self;
            float offsetLength = offset.Length();

            if (offsetLength < float.Epsilon)
                return false;

            span = new RopeSpan(offset / offsetLength, PathLength());
            return true;
        }

        public float PathLength()
        {
            float total = 0f;

            for (int i = 1; i < _positions.Count; i++)
                total += (_positions[i] - _positions[i - 1]).Length();

            return total;
        }

        private void TakeSnapshot()
        {
            _snapshot.Clear();
            _snapshot.AddRange(_positions);
        }

        private void CollideAlongChain(IRopeCollider collider, float radius)
        {
            int last = _positions.Count - 1;

            for (int i = 1; i < last; i++)
                _positions[i] = collider.Resolve(_positions[i - 1], _positions[i], radius);
        }

        private void CollideFromSnapshot(IRopeCollider collider, float radius)
        {
            int last = _positions.Count - 1;

            for (int i = 1; i < last; i++)
                _positions[i] = collider.Resolve(_snapshot[i], _positions[i], radius);
        }

        private void SatisfyConstraints(float restLength)
        {
            int last = _positions.Count - 1;

            for (int i = 0; i < last; i++)
            {
                Vector3 a = _positions[i];
                Vector3 b = _positions[i + 1];
                Vector3 delta = b - a;
                float distance = delta.Length();

                if (distance <= restLength || distance < float.Epsilon)
                    continue;

                Vector3 correction = delta * ((distance - restLength) / distance);
                bool pinnedA = i == 0;
                bool pinnedB = i + 1 == last;

                if (pinnedA && pinnedB)
                    continue;

                if (pinnedA)
                    _positions[i + 1] = b - correction;
                else if (pinnedB)
                    _positions[i] = a + correction;
                else
                {
                    _positions[i] = a + correction * SharedCorrection;
                    _positions[i + 1] = b - correction * SharedCorrection;
                }
            }
        }

        private void FillStraight(int count, Vector3 start, Vector3 end)
        {
            _positions.Clear();
            _previous.Clear();

            for (int i = 0; i < count; i++)
            {
                Vector3 point = Vector3.Lerp(start, end, i / (float)(count - 1));
                _positions.Add(point);
                _previous.Add(point);
            }
        }

        private void Resample(int count)
        {
            _resampled.Clear();
            int oldLast = _positions.Count - 1;

            for (int i = 0; i < count; i++)
            {
                float position = i * oldLast / (float)(count - 1);
                int index = Math.Min((int)position, oldLast - 1);
                _resampled.Add(Vector3.Lerp(_positions[index], _positions[index + 1], position - index));
            }

            _positions.Clear();
            _positions.AddRange(_resampled);
            _previous.Clear();
            _previous.AddRange(_resampled);
        }
    }
}
