using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeLooseness
    {
        public const float Damping = 0.97f;
        public const int Iterations = 8;

        private const float LengthAllowance = 1.02f;
        private const float SharedCorrection = 0.5f;
        private const float MaxStep = 1f / 30f;

        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Vector3> _previous = new List<Vector3>();

        public IReadOnlyList<Vector3> Positions => _positions;

        public void Step(IReadOnlyList<Vector3> targets, IReadOnlyList<float> freedoms, IReadOnlyList<float> spacings, Vector3 gravity, float deltaTime, float radius, IRopeObstacles obstacles)
        {
            if (_positions.Count != targets.Count)
                Reset(targets);

            float step = Math.Clamp(deltaTime, 0f, MaxStep);
            Vector3 fall = gravity * step * step;
            int last = _positions.Count - 1;

            for (int i = 0; i <= last; i++)
            {
                Vector3 current = _positions[i];
                Vector3 next = freedoms[i] <= 0f ? targets[i] : current + (current - _previous[i]) * Damping + fall;
                _previous[i] = freedoms[i] <= 0f ? targets[i] : current;
                _positions[i] = next;
            }

            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                KeepSpacing(spacings, freedoms);
                Tether(targets, freedoms);
            }

            Collide(targets, freedoms, radius, obstacles);
        }

        public void Clear()
        {
            _positions.Clear();
            _previous.Clear();
        }

        public void Reset(IReadOnlyList<Vector3> targets)
        {
            _positions.Clear();
            _previous.Clear();
            _positions.AddRange(targets);
            _previous.AddRange(targets);
        }

        private void KeepSpacing(IReadOnlyList<float> spacings, IReadOnlyList<float> freedoms)
        {
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                float rest = spacings[i] * LengthAllowance;
                Vector3 delta = _positions[i + 1] - _positions[i];
                float distance = delta.Length();

                if (distance <= rest || distance < float.Epsilon)
                    continue;

                Vector3 correction = delta * ((distance - rest) / distance);
                bool pinnedA = freedoms[i] <= 0f;
                bool pinnedB = freedoms[i + 1] <= 0f;

                if (pinnedA && pinnedB)
                    continue;

                if (pinnedA)
                    _positions[i + 1] -= correction;
                else if (pinnedB)
                    _positions[i] += correction;
                else
                {
                    _positions[i] += correction * SharedCorrection;
                    _positions[i + 1] -= correction * SharedCorrection;
                }
            }
        }

        private void Tether(IReadOnlyList<Vector3> targets, IReadOnlyList<float> freedoms)
        {
            for (int i = 0; i < _positions.Count; i++)
            {
                Vector3 offset = _positions[i] - targets[i];
                float distance = offset.Length();

                if (distance > freedoms[i])
                    _positions[i] = distance < float.Epsilon ? targets[i] : targets[i] + offset * (freedoms[i] / distance);
            }
        }

        private void Collide(IReadOnlyList<Vector3> targets, IReadOnlyList<float> freedoms, float radius, IRopeObstacles obstacles)
        {
            if (obstacles == null)
                return;

            for (int i = 0; i < _positions.Count; i++)
            {
                if (freedoms[i] > 0f)
                    _positions[i] = obstacles.Resolve(targets[i], _positions[i], radius);
            }
        }
    }
}
