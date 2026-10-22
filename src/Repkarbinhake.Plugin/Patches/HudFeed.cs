using BepInEx.Configuration;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Visual;
using UnityEngine.InputSystem;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class HudFeed
    {
        private const string KeyboardPrefix = "<Keyboard>";
        private const string DefaultInteractPath = "<Keyboard>/e";

        private readonly LinkRegistry _registry;
        private readonly LinkHoldInput _linkHold;
        private readonly AnchorBoard _anchors;
        private readonly AnchorRopes _anchorRopes;
        private readonly ConfigEntry<string> _linkKey;
        private readonly ConfigEntry<string> _pullKey;
        private readonly ConfigEntry<string> _unwindKey;

        public HudFeed(LinkRegistry registry, LinkHoldInput linkHold, AnchorBoard anchors, AnchorRopes anchorRopes, ConfigEntry<string> linkKey, ConfigEntry<string> pullKey, ConfigEntry<string> unwindKey)
        {
            _registry = registry;
            _linkHold = linkHold;
            _anchors = anchors;
            _anchorRopes = anchorRopes;
            _linkKey = linkKey;
            _pullKey = pullKey;
            _unwindKey = unwindKey;
        }

        public void Publish(int actor)
        {
            RepkaHooks.Hook = _anchors.Has(actor) ? new HookIndicatorState(true, _anchorRopes.IsOwnLocked, _anchorRopes.IsOwnAtMax, _anchorRopes.IsOwnRetracting, _anchorRopes.OwnMeters) : default;
            RepkaHooks.ShowCarabinerReticle = _linkHold.HasAction;
            RepkaHooks.LinkHoldProgress = _linkHold.Progress;
            RepkaHooks.ReleasePrompt = _registry.TryGetHeld(actor, out _) && !_linkHold.HasAction;
            RepkaHooks.PullPrompt = RepkaHooks.ReleasePrompt ? RepkaTexts.Pull : null;
            RepkaHooks.PullKeyPath = _pullKey.Value;
            RepkaHooks.UnwindPrompt = RepkaHooks.ReleasePrompt ? RepkaTexts.Unwind : null;
            RepkaHooks.UnwindKeyPath = _unwindKey.Value;

            if (RepkaHooks.ReleasePrompt)
            {
                RepkaHooks.LinkPrompt = RepkaTexts.Release;
                RepkaHooks.LinkKeyPath = InteractKeyPath();
                return;
            }

            RepkaHooks.LinkPrompt = _linkHold.HasAction ? (_linkHold.TargetIsLinked ? RepkaTexts.Untie : RepkaTexts.Tie) : null;
            RepkaHooks.LinkKeyPath = _linkKey.Value;
        }

        public void Clear()
        {
            RepkaHooks.Hook = default;
            RepkaHooks.ShowCarabinerReticle = false;
            RepkaHooks.LinkHoldProgress = 0f;
            RepkaHooks.LinkPrompt = null;
            RepkaHooks.ReleasePrompt = false;
            RepkaHooks.PullPrompt = null;
            RepkaHooks.UnwindPrompt = null;
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
