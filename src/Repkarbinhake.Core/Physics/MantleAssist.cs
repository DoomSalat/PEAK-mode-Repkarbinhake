using System.Numerics;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Core.Physics
{
    public sealed class MantleAssist
    {
        public const float MaxLipHeight = 1.5f;
        public const float MinLipHeight = -0.5f;
        public const float StuckSpeed = 0.5f;
        public const float StuckSeconds = 0.4f;
        public const float PushSeconds = 2f;
        public const float ProbeHeight = 1.5f;
        public const float ProbeDepth = 3f;
        public const float ProbeRadius = 0.2f;
        public const float WalkableNormal = 0.7f;
        public const float StandHeight = 1f;
        public const float ArriveDistance = 0.3f;

        private float _pushedFor;

        public float StuckFor { get; private set; }
        public bool IsPushing { get; private set; }
        public Vector3 Target { get; private set; }

        public Vector3 Push(Vector3 center, Vector3 velocity, Vector3 lip, Vector3 beyondLip, bool canAssist, float step, float push, float deltaTime, IRopeObstacles obstacles)
        {
            if (!canAssist || step <= 0f || push <= 0f)
            {
                Reset();
                return Vector3.Zero;
            }

            if (IsPushing)
                return KeepPushing(center, push, deltaTime);

            float lipHeight = lip.Y - center.Y;

            if (lipHeight > MaxLipHeight || lipHeight < MinLipHeight || velocity.Length() > StuckSpeed)
            {
                StuckFor = 0f;
                return Vector3.Zero;
            }

            StuckFor += deltaTime;

            if (StuckFor < StuckSeconds || !TryFindFooting(lip, beyondLip, step, obstacles, out Vector3 target))
                return Vector3.Zero;

            Target = target;
            IsPushing = true;
            _pushedFor = 0f;
            return KeepPushing(center, push, deltaTime);
        }

        public void Reset()
        {
            StuckFor = 0f;
            IsPushing = false;
            _pushedFor = 0f;
        }

        private Vector3 KeepPushing(Vector3 center, float push, float deltaTime)
        {
            _pushedFor += deltaTime;
            Vector3 toTarget = Target - center;

            if (_pushedFor > PushSeconds || toTarget.Length() < ArriveDistance)
            {
                Reset();
                return Vector3.Zero;
            }

            return Vector3.Normalize(toTarget) * push;
        }

        private bool TryFindFooting(Vector3 lip, Vector3 beyondLip, float step, IRopeObstacles obstacles, out Vector3 target)
        {
            target = Vector3.Zero;
            Vector3 inward = beyondLip - lip;
            inward.Y = 0f;

            if (inward.LengthSquared() < float.Epsilon)
                return false;

            Vector3 above = lip + Vector3.Normalize(inward) * step + Vector3.UnitY * ProbeHeight;

            if (!obstacles.TryHit(above, above - Vector3.UnitY * ProbeDepth, ProbeRadius, out Vector3 floor, out Vector3 normal) || normal.Y < WalkableNormal)
                return false;

            target = floor + Vector3.UnitY * StandHeight;
            return true;
        }
    }
}
