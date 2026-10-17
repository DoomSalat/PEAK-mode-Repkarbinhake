using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public interface IRopeObstacles : IRopeCollider
    {
        bool TryHit(Vector3 from, Vector3 to, float radius, out Vector3 point, out Vector3 normal);
    }
}
