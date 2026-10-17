using System;
using System.Numerics;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Physics
{
    public sealed class TensionSolver
    {
        private readonly IRopeSettings _settings;

        public TensionSolver(IRopeSettings settings)
        {
            _settings = settings;
        }

        public Vector3 Solve(RopeSpan span, Vector3 selfVelocity, float length, float stability)
        {
            float stretch = span.PathLength - length;

            if (stretch <= 0f)
                return Vector3.Zero;

            float separatingSpeed = MathF.Max(-Vector3.Dot(selfVelocity, span.Direction), 0f);
            float pull = _settings.Stiffness * stretch + _settings.Damping * separatingSpeed;
            pull = MathF.Min(pull, _settings.MaxPull) * (1f - stability);
            return span.Direction * pull;
        }

        public float Load(RopeSpan span, float length)
        {
            float stretch = span.PathLength - length;
            return stretch <= 0f ? 0f : MathF.Min(_settings.Stiffness * stretch, _settings.MaxPull);
        }

        public bool IsSnapped(Vector3 selfPosition, Vector3 partnerPosition)
        {
            return Vector3.DistanceSquared(selfPosition, partnerPosition) > _settings.SnapDistance * _settings.SnapDistance;
        }
    }
}
