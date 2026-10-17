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
        private readonly TestDummy _dummy;
        private readonly KeyBinding _key;
        private readonly ConfigEntry<bool> _enabled;
        private readonly ManualLogSource _log;

        public TestDummyInput(LinkRegistry registry, TestDummy dummy, KeyBinding key, ConfigEntry<bool> enabled, ManualLogSource log)
        {
            _registry = registry;
            _dummy = dummy;
            _key = key;
            _enabled = enabled;
            _log = log;
        }

        public void Tick(int actor, Transform view)
        {
            _dummy.Refresh();

            if (!_dummy.IsPlaced && _registry.TryGet(actor, TestDummy.Actor, out RopeLink lost))
                _registry.Untie(lost);

            if (!_enabled.Value || !_key.WasPressed)
                return;

            if (_dummy.IsPlaced)
            {
                Remove(actor);
                return;
            }

            if (!Physics.Raycast(view.position, view.forward, out RaycastHit hit, PlaceReach, HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap)))
                return;

            _dummy.Place(hit.point);
            _log.LogInfo($"{nameof(TestDummyInput)}: dummy placed at {hit.point}");
        }

        private void Remove(int actor)
        {
            if (!_dummy.IsPlaced)
                return;

            if (_registry.TryGet(actor, TestDummy.Actor, out RopeLink link))
                _registry.Untie(link);

            _dummy.Remove();
        }
    }
}
