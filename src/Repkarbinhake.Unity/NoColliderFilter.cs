using UnityEngine;

namespace Repkarbinhake.Unity
{
    public sealed class NoColliderFilter : IColliderFilter
    {
        public bool Ignores(Collider collider)
        {
            return false;
        }

        public ShellShape ShellOf(Collider collider)
        {
            return ShellShape.None;
        }
    }
}
