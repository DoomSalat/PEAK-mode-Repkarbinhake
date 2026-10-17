using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class LinkHoldInput
    {
        private readonly LinkRegistry _registry;
        private readonly LinkTargetPicker _picker;
        private readonly KeyBinding _key;
        private readonly IRopeSettings _settings;
        private readonly HoldAction _hold = new HoldAction();
        private bool _unties;
        private int _target = HoldAction.NoTarget;

        public LinkHoldInput(LinkRegistry registry, LinkTargetPicker picker, KeyBinding key, IRopeSettings settings)
        {
            _registry = registry;
            _picker = picker;
            _key = key;
            _settings = settings;
        }

        public bool HasTarget => _target != HoldAction.NoTarget;
        public bool TargetIsLinked { get; private set; }
        public float Progress => _hold.Progress;

        public void Tick(Character local, int actor, Footing footing, Transform view, float deltaTime)
        {
            _target = HoldAction.NoTarget;

            if (local.CanDoInput() && footing != Footing.Unconscious && _picker.TryPick(local, view, out int target))
                _target = target;

            TargetIsLinked = HasTarget && _registry.TryGet(actor, _target, out _);

            if (_key.WasPressed && HasTarget)
            {
                _unties = TargetIsLinked;
                _hold.Begin(_target, _unties ? _settings.UntieHoldSeconds : _settings.TieHoldSeconds);
            }

            if (_hold.Advance(_key.IsPressed, _target, deltaTime))
                Complete(actor, _target);
        }

        public void Cancel()
        {
            _target = HoldAction.NoTarget;
            _hold.Cancel();
        }

        private void Complete(int actor, int partner)
        {
            bool isLinked = _registry.TryGet(actor, partner, out RopeLink link);

            if (_unties && isLinked)
                _registry.Untie(link);
            else if (!_unties && !isLinked)
                _registry.Tie(actor, partner);
        }
    }
}
