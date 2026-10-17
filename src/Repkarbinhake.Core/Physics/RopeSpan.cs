using System.Numerics;

namespace Repkarbinhake.Core.Physics
{
    public readonly struct RopeSpan
    {
        public RopeSpan(Vector3 direction, float pathLength)
        {
            Direction = direction;
            PathLength = pathLength;
        }

        public Vector3 Direction { get; }
        public float PathLength { get; }

        public static RopeSpan Straight(Vector3 self, Vector3 partner) // Static: factory for the obstacle-free case
        {
            Vector3 offset = partner - self;
            float distance = offset.Length();
            return new RopeSpan(distance > 0f ? offset / distance : Vector3.Zero, distance);
        }
    }
}
