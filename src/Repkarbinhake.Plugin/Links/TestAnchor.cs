using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class TestAnchor
    {
        public const int Actor = -1;

        public bool IsPlaced { get; private set; }
        public Vector3 Position { get; private set; }

        public void Place(Vector3 position)
        {
            Position = position;
            IsPlaced = true;
        }

        public void Remove()
        {
            IsPlaced = false;
        }
    }
}
