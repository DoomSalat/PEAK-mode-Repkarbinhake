using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class ReelDriver
    {
        private const float PublishInterval = 0.1f;
        private const float ButtonScroll = 0.4f;
        private const float Smoothing = 4f;
        private const float MinVelocity = -1f;
        private const float MaxVelocity = 5f;
        private const float IdleVelocity = 0.001f;

        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly TensionSolver _tension;
        private readonly RopePresenterPool _ropes;
        private readonly TeamPull _team;
        private float _publishTimer;
        private float _velocity;
        private bool _changed;

        public ReelDriver(LinkRegistry registry, RopeReel reel, TensionSolver tension, RopePresenterPool ropes, TeamPull team)
        {
            _registry = registry;
            _reel = reel;
            _tension = tension;
            _ropes = ropes;
            _team = team;
        }

        public bool IsReelingIn { get; private set; }

        public void Tick(Character local, int actor, Footing footing, bool pulling, bool unwinding, float deltaTime)
        {
            float target = ScrollTarget(local.input, pulling, unwinding);
            IsReelingIn = target < 0f;
            _velocity = Mathf.Clamp(Mathf.Lerp(_velocity, target, deltaTime * Smoothing), MinVelocity, MaxVelocity);

            if (Mathf.Abs(_velocity) > IdleVelocity)
                Reel(local, actor, footing, _velocity, deltaTime);

            Publish(actor, deltaTime);
        }

        public void Stop()
        {
            _velocity = 0f;
            IsReelingIn = false;
        }

        private float ScrollTarget(CharacterInput input, bool pulling, bool unwinding)
        {
            if (pulling)
                return -ButtonScroll;

            if (unwinding)
                return ButtonScroll;

            if (input.scrollForwardIsPressed)
                return ButtonScroll;

            if (input.scrollBackwardIsPressed)
                return -ButtonScroll;

            return input.scrollInput;
        }

        private void Reel(Character local, int actor, Footing footing, float velocity, float deltaTime)
        {
            foreach (RopeLink link in _registry.Links)
            {
                if (!link.IsHeldBy(actor))
                    continue;

                float load = _team.SharedLoad(link, actor, _registry.Links, _tension.Load(_ropes.SpanFor(link, actor), _reel.EffectiveLength(link)));
                ReelStep step = _reel.StepReel(link, actor, velocity, deltaTime, load, footing, local.data.currentStamina);

                if (step.StaminaCost > 0f)
                    local.UseStamina(step.StaminaCost, useBonusStamina: false);

                _registry.ReelLocally(link, step.Reel);
                _changed = true;
            }
        }

        private void Publish(int actor, float deltaTime)
        {
            _publishTimer += deltaTime;

            if (!_changed || _publishTimer < PublishInterval)
                return;

            _publishTimer = 0f;
            _changed = false;

            foreach (RopeLink link in _registry.Links)
            {
                if (link.IsHeldBy(actor))
                    _registry.PublishOwnEnd(link);
            }
        }
    }
}
