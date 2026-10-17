using System.Numerics;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Core.Physics
{
    public sealed class LedgeAssist
    {
        public const float StuckSpeed = 0.3f;
        public const float StuckSeconds = 0.3f;
        public const float ProbeDistance = 0.6f;
        public const float ProbeRadius = 0.2f;

        public float StuckFor { get; private set; }
        public bool IsPushing { get; private set; }

        public Vector3 Adjust(Vector3 pull, Vector3 center, Vector3 velocity, bool canAssist, float push, float deltaTime, IRopeObstacles obstacles)
        {
            IsPushing = false;

            if (!canAssist || pull.LengthSquared() < float.Epsilon)
            {
                StuckFor = 0f;
                return pull;
            }

            Vector3 direction = Vector3.Normalize(pull);

            if (Vector3.Dot(velocity, direction) > StuckSpeed)
            {
                StuckFor = 0f;
                return pull;
            }

            StuckFor += deltaTime;

            if (StuckFor < StuckSeconds || !obstacles.TryHit(center, center + direction * ProbeDistance, ProbeRadius, out _, out Vector3 normal))
                return pull;

            float into = Vector3.Dot(pull, normal);

            if (into >= 0f)
                return pull;

            IsPushing = true;
            return pull - normal * into + normal * push;
        }
    }
}
