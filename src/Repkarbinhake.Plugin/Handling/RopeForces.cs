using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
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
        private readonly TensionSolver _anchorTension;
        private readonly RopeReel _reel;
        private readonly ActorStates _actors;
        private readonly RopePresenterPool _ropes;
        private readonly RepkaConfig _config;
        private readonly IRopeObstacles _obstacles;
        private readonly LedgeAssist _ledge;
        private readonly List<RopeLink> _snapped = new List<RopeLink>();

        public RopeForces(LinkRegistry registry, StabilityResolver stability, TensionSolver tension, TensionSolver anchorTension, RopeReel reel, ActorStates actors, RopePresenterPool ropes, RepkaConfig config, IRopeObstacles obstacles, LedgeAssist ledge)
        {
            _registry = registry;
            _stability = stability;
            _tension = tension;
            _anchorTension = anchorTension;
            _reel = reel;
            _actors = actors;
            _ropes = ropes;
            _config = config;
            _obstacles = obstacles;
            _ledge = ledge;
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

                if (!AnchorActors.IsAnchor(partner) && _tension.IsSnapped(local.Center.ToNumerics(), partnerPoint.ToNumerics()))
                {
                    _snapped.Add(link);
                    continue;
                }

                partnerHolds |= link.IsHeldBy(partner);
                TensionSolver tension = AnchorActors.IsAnchor(partner) ? _anchorTension : _tension;
                total += tension.Solve(_ropes.SpanFor(link, actor), local.data.avarageVelocity.ToNumerics(), _reel.EffectiveLength(link), stability).ToUnity();
            }

            foreach (RopeLink link in _snapped)
                _registry.Untie(link);

            Footing footing = _actors.FootingOf(actor);
            bool canAssist = _config.LedgePush.Value > 0f && (footing == Footing.Airborne || footing == Footing.Unconscious);
            total = _ledge.Adjust(total.ToNumerics(), local.Center.ToNumerics(), local.data.avarageVelocity.ToNumerics(), canAssist, _config.LedgePush.Value, Time.fixedDeltaTime, _obstacles).ToUnity();

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
