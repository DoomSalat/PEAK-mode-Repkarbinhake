using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public interface IRopeCollider
    {
        Vector3 Resolve(Vector3 from, Vector3 to, float radius);
    }
}
