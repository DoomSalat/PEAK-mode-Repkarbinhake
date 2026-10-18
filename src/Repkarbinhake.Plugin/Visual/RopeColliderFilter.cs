using System;
using System.Collections.Generic;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeColliderFilter : IColliderFilter
    {
        private const string FoliageMarker = "Foliage";
        private const float SlabRatio = 1f / 3f;

        private static readonly string[] FloorMarkers = { "Floor", "maze" }; // Static: fixed name markers of citadel floor objects

        private readonly Dictionary<int, bool> _known = new Dictionary<int, bool>();
        private readonly Dictionary<int, ShellShape> _shells = new Dictionary<int, ShellShape>();

        public bool Ignores(Collider collider)
        {
            int id = collider.GetInstanceID();

            if (_known.TryGetValue(id, out bool ignored))
                return ignored;

            ignored = IsChest(collider) || collider.GetComponentInParent<ScoutCannon>() != null;
            _known[id] = ignored;
            return ignored;
        }

        public ShellShape ShellOf(Collider collider)
        {
            int id = collider.GetInstanceID();

            if (_shells.TryGetValue(id, out ShellShape shell))
                return shell;

            shell = Classify(collider);
            _shells[id] = shell;
            return shell;
        }

        public void Clear()
        {
            _known.Clear();
            _shells.Clear();
        }

        private ShellShape Classify(Collider collider)
        {
            if (collider.GetComponentInParent<BreakableBridge>() != null)
                return ShellShape.Top;

            if (!(collider is MeshCollider mesh) || mesh.convex)
                return ShellShape.None;

            if (IsFoliage(collider.GetComponent<Renderer>()))
                return ShellShape.Round;

            return IsFloorName(collider.name) && IsThin(collider.bounds) ? ShellShape.Slab : ShellShape.None;
        }

        private bool IsFloorName(string name)
        {
            foreach (string marker in FloorMarkers)
            {
                if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private bool IsThin(Bounds bounds)
        {
            Vector3 size = bounds.size;
            float thinnest = Mathf.Min(size.x, Mathf.Min(size.y, size.z));
            float widest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            return thinnest < widest * SlabRatio;
        }

        private bool IsFoliage(Renderer renderer)
        {
            return renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.name.Contains(FoliageMarker);
        }

        private bool IsChest(Collider collider)
        {
            return collider.GetComponentInParent<Luggage>() != null || collider.GetComponentInParent<MirageLuggage>() != null;
        }
    }
}
