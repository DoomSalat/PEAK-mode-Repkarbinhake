using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class RepkaHookFeed
    {
        private const string KeyboardPrefix = "<Keyboard>";
        private const string DefaultInteractPath = "<Keyboard>/e";

        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly RopeHandPolicy _policy;
        private readonly RopePresenterPool _ropes;
        private readonly LinkHoldInput _linkHold;
        private readonly AnchorBoard _anchors;
        private readonly LinkColors _colors;
        private readonly ConfigEntry<float> _ropeColor;
        private readonly ConfigEntry<string> _linkKey;

        public RepkaHookFeed(LinkRegistry registry, RopeReel reel, RopeHandPolicy policy, RopePresenterPool ropes, LinkHoldInput linkHold, AnchorBoard anchors, LinkColors colors, ConfigEntry<float> ropeColor, ConfigEntry<string> linkKey)
        {
            _registry = registry;
            _reel = reel;
            _policy = policy;
            _ropes = ropes;
            _linkHold = linkHold;
            _anchors = anchors;
            _colors = colors;
            _ropeColor = ropeColor;
            _linkKey = linkKey;
        }

        public void Publish(int actor, Footing footing, Transform view)
        {
            RepkaHooks.HookPlaced = _anchors.Has(actor);
            RepkaHooks.ShowCarabinerReticle = _linkHold.HasTarget;
            RepkaHooks.LinkHoldProgress = _linkHold.Progress;
            bool holds = _registry.TryGetHeld(actor, out RopeLink heldLink);
            RepkaHooks.ReleasePrompt = holds && !_linkHold.HasTarget;

            if (RepkaHooks.ReleasePrompt)
            {
                RepkaHooks.LinkPrompt = RepkaTexts.Release;
                RepkaHooks.LinkKeyPath = InteractKeyPath();
            }
            else
            {
                RepkaHooks.LinkPrompt = _linkHold.HasTarget ? (_linkHold.TargetIsLinked ? RepkaTexts.Untie : RepkaTexts.Tie) : null;
                RepkaHooks.LinkKeyPath = _linkKey.Value;
            }

            RepkaHooks.HeldRopeLength = holds ? _reel.EffectiveLength(heldLink) : 0f;
            RepkaHooks.HeldRopeColor = holds && _ropeColor.Value > 0f && _colors.TryGetColor(heldLink, out Color color) ? color : (Color?)null;
            RepkaHooks.HeldRopeTint = _ropeColor.Value;
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
            RepkaHooks.HookPlaced = false;
            RepkaHooks.ShowCarabinerReticle = false;
            RepkaHooks.LinkHoldProgress = 0f;
            RepkaHooks.LinkPrompt = null;
            RepkaHooks.ReleasePrompt = false;
            RepkaHooks.HeldRopeLength = 0f;
            RepkaHooks.HeldRopeColor = null;
        }

        private string InteractKeyPath()
        {
            foreach (InputBinding binding in CharacterInput.action_interact.bindings)
            {
                if (binding.effectivePath.StartsWith(KeyboardPrefix))
                    return binding.effectivePath;
            }

            return DefaultInteractPath;
        }
    }
}
