using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Handling;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class AssistFeed
    {
        private readonly LinkRegistry _registry;
        private readonly AutoHandUp _handUp;
        private readonly ConfigEntry<bool> _stableCamera;

        public AssistFeed(LinkRegistry registry, AutoHandUp handUp, ConfigEntry<bool> stableCamera)
        {
            _registry = registry;
            _handUp = handUp;
            _stableCamera = stableCamera;
        }

        public void Publish(int actor, Footing footing, bool hanging)
        {
            RepkaHooks.CameraHanging = _stableCamera.Value && hanging;
            RepkaHooks.StableCamera = _stableCamera.Value && IsTied(actor);
            RepkaHooks.HandUpTarget = _handUp.Target;
            RepkaHooks.LocalNeedsHand = AutoHandUp.IsHelpless(footing) || footing == Footing.Climbing;
        }

        public void Clear()
        {
            RepkaHooks.StableCamera = false;
            RepkaHooks.CameraHanging = false;
            RepkaHooks.HandUpTarget = 0;
            RepkaHooks.LocalNeedsHand = false;
        }

        private bool IsTied(int actor)
        {
            foreach (RopeLink link in _registry.Links)
            {
                if (link.Contains(actor))
                    return true;
            }

            return false;
        }
    }
}
