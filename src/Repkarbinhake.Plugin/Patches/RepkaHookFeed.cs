using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class RepkaHookFeed
    {
        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly RopeHandPolicy _policy;
        private readonly RopePresenterPool _ropes;
        private readonly LinkHoldInput _linkHold;
        private readonly ConfigEntry<string> _linkKey;

        public RepkaHookFeed(LinkRegistry registry, RopeReel reel, RopeHandPolicy policy, RopePresenterPool ropes, LinkHoldInput linkHold, ConfigEntry<string> linkKey)
        {
            _registry = registry;
            _reel = reel;
            _policy = policy;
            _ropes = ropes;
            _linkHold = linkHold;
            _linkKey = linkKey;
        }

        public void Publish(int actor, Footing footing, Transform view)
        {
            RepkaHooks.ShowCarabinerReticle = _linkHold.HasTarget;
            RepkaHooks.LinkHoldProgress = _linkHold.Progress;
            RepkaHooks.LinkPrompt = _linkHold.HasTarget ? (_linkHold.TargetIsLinked ? RepkaTexts.Untie : RepkaTexts.Tie) : null;
            RepkaHooks.LinkKeyPath = _linkKey.Value;
            RepkaHooks.HeldRopeLength = _registry.TryGetHeld(actor, out RopeLink heldLink) ? _reel.EffectiveLength(heldLink) : 0f;
            RepkaHooks.HeldRope = _ropes.TryGetHeld(actor, out RopeInteractible held) ? held : null;

            if (!_linkHold.HasTarget && _policy.CanHold(footing) && _ropes.TryAim(new Ray(view.position, view.forward), actor, out RopeInteractible aimed, out _))
            {
                RepkaHooks.AimedRope = aimed;
                return;
            }

            RepkaHooks.AimedRope = null;
        }

        public void Clear()
        {
            RepkaHooks.HeldRope = null;
            RepkaHooks.AimedRope = null;
            RepkaHooks.ShowCarabinerReticle = false;
            RepkaHooks.LinkHoldProgress = 0f;
            RepkaHooks.LinkPrompt = null;
            RepkaHooks.HeldRopeLength = 0f;
        }
    }
}
