using System.Collections.Generic;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class PassThroughColliderFilter : IColliderFilter
    {
        private readonly Dictionary<int, bool> _known = new Dictionary<int, bool>();

        public bool Ignores(Collider collider)
        {
            int id = collider.GetInstanceID();

            if (_known.TryGetValue(id, out bool ignored))
                return ignored;

            ignored = IsChest(collider) || collider.GetComponentInParent<BreakableBridge>() != null || collider.GetComponentInParent<ScoutCannon>() != null;
            _known[id] = ignored;
            return ignored;
        }

        public void Clear()
        {
            _known.Clear();
        }

        private bool IsChest(Collider collider)
        {
            return collider.GetComponentInParent<Luggage>() != null || collider.GetComponentInParent<MirageLuggage>() != null;
        }
    }
}
