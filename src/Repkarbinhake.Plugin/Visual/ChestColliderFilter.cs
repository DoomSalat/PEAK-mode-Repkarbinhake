using System.Collections.Generic;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class ChestColliderFilter : IColliderFilter
    {
        private readonly Dictionary<int, bool> _known = new Dictionary<int, bool>();

        public bool Ignores(Collider collider)
        {
            int id = collider.GetInstanceID();

            if (_known.TryGetValue(id, out bool isChest))
                return isChest;

            isChest = collider.GetComponentInParent<Luggage>() != null || collider.GetComponentInParent<MirageLuggage>() != null;
            _known[id] = isChest;
            return isChest;
        }

        public void Clear()
        {
            _known.Clear();
        }
    }
}
