using BepInEx.Configuration;
using BepInEx.Logging;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class TestAnchorInput
    {
        private const float PlaceReach = 30f;
        private const float SurfaceOffset = 0.15f;

        private readonly LinkRegistry _registry;
        private readonly TestAnchor _anchor;
        private readonly KeyBinding _key;
        private readonly ConfigEntry<bool> _enabled;
        private readonly ManualLogSource _log;

        public TestAnchorInput(LinkRegistry registry, TestAnchor anchor, KeyBinding key, ConfigEntry<bool> enabled, ManualLogSource log)
        {
            _registry = registry;
            _anchor = anchor;
            _key = key;
            _enabled = enabled;
            _log = log;
        }

        public void Tick(int actor, Transform view)
        {
            if (!_enabled.Value || !_key.WasPressed)
                return;

            if (_anchor.IsPlaced)
            {
                Remove(actor);
                return;
            }

            if (!Physics.Raycast(view.position, view.forward, out RaycastHit hit, PlaceReach, HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap)))
                return;

            _anchor.Place(hit.point + hit.normal * SurfaceOffset);
            _registry.Tie(actor, TestAnchor.Actor);
            _log.LogInfo($"{nameof(TestAnchorInput)}: anchor placed at {_anchor.Position}");
        }

        private void Remove(int actor)
        {
            if (_registry.TryGet(actor, TestAnchor.Actor, out RopeLink link))
                _registry.Untie(link);

            _anchor.Remove();
        }
    }
}
