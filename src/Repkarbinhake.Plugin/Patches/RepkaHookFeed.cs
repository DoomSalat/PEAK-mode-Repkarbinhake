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
        private readonly AnchorRopes _anchorRopes;
        private readonly LinkColors _colors;
        private readonly ConfigEntry<float> _ropeColor;
        private readonly ConfigEntry<string> _linkKey;
        private readonly ConfigEntry<string> _pullKey;
        private readonly ConfigEntry<string> _unwindKey;
        private readonly ConfigEntry<bool> _stableCamera;

        public RepkaHookFeed(LinkRegistry registry, RopeReel reel, RopeHandPolicy policy, RopePresenterPool ropes, LinkHoldInput linkHold, AnchorBoard anchors, AnchorRopes anchorRopes, LinkColors colors, ConfigEntry<float> ropeColor, ConfigEntry<string> linkKey, ConfigEntry<string> pullKey, ConfigEntry<string> unwindKey, ConfigEntry<bool> stableCamera)
        {
            _registry = registry;
            _reel = reel;
            _policy = policy;
            _ropes = ropes;
            _linkHold = linkHold;
            _anchors = anchors;
            _anchorRopes = anchorRopes;
            _colors = colors;
            _ropeColor = ropeColor;
            _linkKey = linkKey;
            _pullKey = pullKey;
            _unwindKey = unwindKey;
            _stableCamera = stableCamera;
        }

        public void Publish(int actor, Footing footing, Transform view)
        {
            RepkaHooks.HookPlaced = _anchors.Has(actor);
            RepkaHooks.HookLocked = RepkaHooks.HookPlaced && _anchorRopes.IsOwnLocked;
            RepkaHooks.HookAtMax = RepkaHooks.HookPlaced && _anchorRopes.IsOwnAtMax;
            RepkaHooks.HookRetracting = RepkaHooks.HookPlaced && _anchorRopes.IsOwnRetracting;
            RepkaHooks.HookMeters = _anchorRopes.OwnMeters;
            RepkaHooks.StableCamera = _stableCamera.Value && IsTied(actor);
            RepkaHooks.ShowCarabinerReticle = _linkHold.HasAction;
            RepkaHooks.LinkHoldProgress = _linkHold.Progress;
            bool holds = _registry.TryGetHeld(actor, out RopeLink heldLink);
            RepkaHooks.ReleasePrompt = holds && !_linkHold.HasAction;
            RepkaHooks.PullPrompt = RepkaHooks.ReleasePrompt ? RepkaTexts.Pull : null;
            RepkaHooks.PullKeyPath = _pullKey.Value;
            RepkaHooks.UnwindPrompt = RepkaHooks.ReleasePrompt ? RepkaTexts.Unwind : null;
            RepkaHooks.UnwindKeyPath = _unwindKey.Value;

            if (RepkaHooks.ReleasePrompt)
            {
                RepkaHooks.LinkPrompt = RepkaTexts.Release;
                RepkaHooks.LinkKeyPath = InteractKeyPath();
            }
            else
            {
                RepkaHooks.LinkPrompt = _linkHold.HasAction ? (_linkHold.TargetIsLinked ? RepkaTexts.Untie : RepkaTexts.Tie) : null;
                RepkaHooks.LinkKeyPath = _linkKey.Value;
            }

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
            RepkaHooks.HookPlaced = false;
            RepkaHooks.HookLocked = false;
            RepkaHooks.HookAtMax = false;
            RepkaHooks.HookRetracting = false;
            RepkaHooks.HookMeters = 0f;
            RepkaHooks.StableCamera = false;
            RepkaHooks.ShowCarabinerReticle = false;
            RepkaHooks.LinkHoldProgress = 0f;
            RepkaHooks.LinkPrompt = null;
            RepkaHooks.ReleasePrompt = false;
            RepkaHooks.PullPrompt = null;
            RepkaHooks.UnwindPrompt = null;
            RepkaHooks.HeldRopeLength = 0f;
            RepkaHooks.HeldRopeColor = null;
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
