using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class HeldRopeFeed
    {
        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly RopeHandPolicy _policy;
        private readonly RopePresenterPool _ropes;
        private readonly LinkHoldInput _linkHold;
        private readonly LinkColors _colors;
        private readonly ConfigEntry<float> _ropeColor;

        public HeldRopeFeed(LinkRegistry registry, RopeReel reel, RopeHandPolicy policy, RopePresenterPool ropes, LinkHoldInput linkHold, LinkColors colors, ConfigEntry<float> ropeColor)
        {
            _registry = registry;
            _reel = reel;
            _policy = policy;
            _ropes = ropes;
            _linkHold = linkHold;
            _colors = colors;
            _ropeColor = ropeColor;
        }

        public void Publish(int actor, Footing footing, Transform view)
        {
            CollectPartners(actor);
            bool holds = _registry.TryGetHeld(actor, out RopeLink heldLink);
            RepkaHooks.HeldRopeLength = holds ? _reel.EffectiveLength(heldLink) : 0f;
            RepkaHooks.HeldRopeColor = holds && _ropeColor.Value > 0f && _colors.TryGetColor(heldLink, out Color color) ? color : (Color?)null;
            RepkaHooks.HeldRopeTint = _ropeColor.Value;
            RepkaHooks.HeldRope = _ropes.TryGetHeld(actor, out RopeInteractible held) ? held : null;

            if (!_linkHold.HasAction && _policy.CanHold(footing) && _ropes.TryAim(new Ray(view.position, view.forward), actor, out RopeInteractible aimed, out _))
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
            RepkaHooks.TiedPartners.Clear();
            RepkaHooks.HeldRopeLength = 0f;
            RepkaHooks.HeldRopeColor = null;
        }

        private void CollectPartners(int actor)
        {
            RepkaHooks.TiedPartners.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                if (link.Contains(actor) && !AnchorActors.IsAnchor(link.FirstActor))
                    RepkaHooks.TiedPartners.Add(link.Partner(actor));
            }
        }
    }
}
