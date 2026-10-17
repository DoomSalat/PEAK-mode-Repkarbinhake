using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public interface IRopeObstacles
    {
        Vector3 Resolve(Vector3 from, Vector3 to, float radius);
        bool TryHit(Vector3 from, Vector3 to, float radius, out Vector3 point, out Vector3 normal);
    }
}
