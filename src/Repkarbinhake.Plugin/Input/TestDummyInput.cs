using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class TestDummyInput
    {
        private const float PlaceReach = 30f;

        private readonly LinkRegistry _registry;
        private readonly TestDummies _dummies;
        private readonly KeyBinding _key;
        private readonly ConfigEntry<int> _count;
        private readonly ManualLogSource _log;
        private readonly List<RopeLink> _lost = new List<RopeLink>();

        public TestDummyInput(LinkRegistry registry, TestDummies dummies, KeyBinding key, ConfigEntry<int> count, ManualLogSource log)
        {
            _registry = registry;
            _dummies = dummies;
            _key = key;
            _count = count;
            _log = log;
        }

        public void Tick(int actor, Transform view)
        {
            _dummies.Refresh();
            UntieMissing(actor);

            if (!_key.WasPressed)
                return;

            if (_dummies.Placed.Count > 0)
            {
                _dummies.RemoveAll();
                UntieMissing(actor);
                return;
            }

            if (!Physics.Raycast(view.position, view.forward, out RaycastHit hit, PlaceReach, HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap)))
                return;

            _dummies.PlaceRing(hit.point, _count.Value);
            _log.LogInfo($"{nameof(TestDummyInput)}: {_count.Value} dummies placed around {hit.point}");
        }

        private void UntieMissing(int actor)
        {
            _lost.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                int partner = link.Contains(actor) ? link.Partner(actor) : 0;

                if (_dummies.IsDummy(partner) && !_dummies.TryGet(partner, out _))
                    _lost.Add(link);
            }

            foreach (RopeLink link in _lost)
                _registry.Untie(link);
        }
    }
}
