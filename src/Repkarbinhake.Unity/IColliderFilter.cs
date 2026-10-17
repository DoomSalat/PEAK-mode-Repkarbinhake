using UnityEngine;

namespace Repkarbinhake.Unity
{
    public interface IColliderFilter
    {
        bool Ignores(Collider collider);
    }
}
