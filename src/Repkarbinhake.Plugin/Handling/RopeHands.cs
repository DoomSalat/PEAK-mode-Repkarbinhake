using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Links;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeHands
    {
        private const float GrabGraceSeconds = 0.5f;

        private readonly LinkRegistry _registry;
        private readonly RopeHandPolicy _policy;
        private readonly ActorStates _actors;
        private float _grabGrace;

        public RopeHands(LinkRegistry registry, RopeHandPolicy policy, ActorStates actors)
        {
            _registry = registry;
            _policy = policy;
            _actors = actors;
        }

        public bool IsHolding(int actor)
        {
            foreach (RopeLink link in _registry.Links)
            {
                if (link.IsHeldBy(actor))
                    return true;
            }

            return false;
        }

        public void Toggle(Character local, RopeLink link)
        {
            int actor = _registry.LocalActor;

            if (link.IsHeldBy(actor))
            {
                _registry.Release(link);
                return;
            }

            if (local == null || !_policy.CanHold(_actors.FootingOf(actor)))
                return;

            if (local.data.currentItem != null)
                local.refs.items.EquipSlot(Optionable<byte>.None);

            _registry.ReleaseAllHeld();
            _registry.Grab(link);
            _grabGrace = GrabGraceSeconds;
        }

        public void Enforce(Character local, int actor, Footing footing, float deltaTime)
        {
            _grabGrace -= deltaTime;

            if (!IsHolding(actor))
                return;

            bool itemInHands = _grabGrace <= 0f && local.data.currentItem != null;

            if (!_policy.CanHold(footing) || itemInHands)
                _registry.ReleaseAllHeld();
        }
    }
}
