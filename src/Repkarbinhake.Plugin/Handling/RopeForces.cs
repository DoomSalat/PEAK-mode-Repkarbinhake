using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeForces
    {
        private readonly LinkRegistry _registry;
        private readonly StabilityResolver _stability;
        private readonly TensionSolver _tension;
        private readonly RopeReel _reel;
        private readonly ActorStates _actors;
        private readonly RopePresenterPool _ropes;
        private readonly RepkaConfig _config;
        private readonly List<RopeLink> _snapped = new List<RopeLink>();

        public RopeForces(LinkRegistry registry, StabilityResolver stability, TensionSolver tension, RopeReel reel, ActorStates actors, RopePresenterPool ropes, RepkaConfig config)
        {
            _registry = registry;
            _stability = stability;
            _tension = tension;
            _reel = reel;
            _actors = actors;
            _ropes = ropes;
            _config = config;
        }

        public void Apply(Character local, int actor)
        {
            float stability = _stability.Resolve(actor, _registry.Links);
            Vector3 total = Vector3.zero;
            bool partnerHolds = false;
            _snapped.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                if (!link.Contains(actor))
                    continue;

                int partner = link.Partner(actor);

                if (!_actors.TryGetRopePoint(partner, link.IsHeldBy(partner), out Vector3 partnerPoint))
                    continue;

                if (_tension.IsSnapped(local.Center.ToNumerics(), partnerPoint.ToNumerics()))
                {
                    _snapped.Add(link);
                    continue;
                }

                partnerHolds |= link.IsHeldBy(partner);
                total += _tension.Solve(_ropes.SpanFor(link, actor), local.data.avarageVelocity.ToNumerics(), _reel.EffectiveLength(link), stability).ToUnity();
            }

            foreach (RopeLink link in _snapped)
                _registry.Untie(link);

            if (total.sqrMagnitude > 0f)
                local.AddForce(total);

            KeepHangingLight(local, total);

            if (partnerHolds)
                Belay(local);
        }

        private void KeepHangingLight(Character local, Vector3 pull)
        {
            if (!local.data.isGrounded && pull.y >= _config.SupportLoadThreshold.Value)
                local.ClampSinceGrounded(_config.HangAirTimeCap.Value);
        }

        private void Belay(Character local)
        {
            if (!local.data.isClimbing || !local.OutOfStamina())
                return;

            local.data.outOfStaminaClimbingFor = Mathf.Min(local.data.outOfStaminaClimbingFor, _config.BelaySlideCap.Value);
            local.ClampSinceGrounded(_config.HangAirTimeCap.Value);
        }
    }
}
