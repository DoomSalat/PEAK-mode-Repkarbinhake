using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class TestDummies
    {
        public const int FirstActor = -1000000;

        private const float RingRadius = 2.5f;
        private const float ProbeHeight = 3f;
        private const float ProbeDepth = 10f;

        private readonly List<TestDummy> _placed = new List<TestDummy>();

        public IReadOnlyList<TestDummy> Placed => _placed;

        public bool IsDummy(int actor)
        {
            return actor <= FirstActor;
        }

        public bool TryGet(int actor, out TestDummy dummy)
        {
            foreach (TestDummy placed in _placed)
            {
                if (placed.Actor == actor)
                {
                    dummy = placed;
                    return true;
                }
            }

            dummy = null;
            return false;
        }

        public void Refresh()
        {
            foreach (TestDummy dummy in _placed)
                dummy.Refresh();
        }

        public void PlaceRing(Vector3 center, int count)
        {
            int mask = HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap);

            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.PI * 2f * i / count;
                Vector3 spot = count == 1 ? center : center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * RingRadius;
                Vector3 ground = Physics.Raycast(spot + Vector3.up * ProbeHeight, Vector3.down, out RaycastHit hit, ProbeDepth, mask) ? hit.point : spot;
                TestDummy dummy = new TestDummy(FirstActor - i);
                dummy.Place(ground);
                _placed.Add(dummy);
            }
        }

        public void RemoveAll()
        {
            foreach (TestDummy dummy in _placed)
                dummy.Remove();

            _placed.Clear();
        }
    }
}
