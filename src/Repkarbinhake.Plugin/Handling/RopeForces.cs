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
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeForces
    {
        private const float HandsAboveFloor = 1f;

        private readonly LinkRegistry _registry;
        private readonly StabilityResolver _stability;
        private readonly TensionSolver _tension;
        private readonly TensionSolver _anchorTension;
        private readonly TensionSolver _haulTension;
        private readonly ReelInWatch _reelWatch = new ReelInWatch();
        private readonly LinkLengths _lengths;
        private readonly ActorStates _actors;
        private readonly RopePresenterPool _ropes;
        private readonly RepkaConfig _config;
        private readonly IRopeObstacles _obstacles;
        private readonly LedgeAssist _ledge;
        private readonly MantleAssist _mantle;
        private readonly List<RopeLink> _snapped = new List<RopeLink>();

        public RopeForces(LinkRegistry registry, StabilityResolver stability, TensionSolver tension, TensionSolver anchorTension, TensionSolver haulTension, LinkLengths lengths, ActorStates actors, RopePresenterPool ropes, RepkaConfig config, IRopeObstacles obstacles, LedgeAssist ledge, MantleAssist mantle)
        {
            _registry = registry;
            _stability = stability;
            _tension = tension;
            _anchorTension = anchorTension;
            _haulTension = haulTension;
            _lengths = lengths;
            _actors = actors;
            _ropes = ropes;
            _config = config;
            _obstacles = obstacles;
            _ledge = ledge;
            _mantle = mantle;
        }

        public bool IsPulledByTeammate { get; private set; }
        public bool IsOnTautRope { get; private set; }

        public void Apply(Character local, int actor)
        {
            IsPulledByTeammate = false;
            IsOnTautRope = false;
            float stability = _stability.Resolve(actor, _registry.Links);
            Vector3 total = Vector3.zero;
            bool partnerHolds = false;
            bool hauled = false;
            bool climbHauled = false;
            Footing footing = _actors.FootingOf(actor);
            NumericsVector3 lip = NumericsVector3.Zero;
            NumericsVector3 beyondLip = NumericsVector3.Zero;
            _snapped.Clear();
            _reelWatch.Begin();

            foreach (RopeLink link in _registry.Links)
            {
                if (!link.Contains(actor))
                    continue;

                int partner = link.Partner(actor);
                bool hook = AnchorActors.IsAnchor(partner);
                bool held = link.IsHeldBy(partner);
                bool reeledIn = !hook && _reelWatch.IsReelingIn(link, partner, Time.fixedDeltaTime);

                if (!_actors.TryGetRopePoint(partner, held, out Vector3 partnerPoint))
                    continue;

                if (!hook && _tension.IsSnapped(local.Center.ToNumerics(), partnerPoint.ToNumerics()))
                {
                    _snapped.Add(link);
                    continue;
                }

                partnerHolds |= held;
                TensionSolver tension = hook ? _anchorTension : held && reeledIn ? _haulTension : _tension;
                RopeSpan span = _ropes.SpanFor(link, actor);
                float length = _lengths.EffectiveLength(link);
                total += tension.Solve(span, local.data.avarageVelocity.ToNumerics(), length, stability).ToUnity();
                bool loaded = tension.Load(span, length) > 0f;
                bool taut = (hook || held) && loaded;
                IsOnTautRope |= loaded;
                IsPulledByTeammate |= taut && !hook;

                if (!hauled && taut)
                    hauled = TryGetLip(link, actor, partnerPoint.ToNumerics(), hook ? 0f : HandsAboveFloor, local.Center.ToNumerics(), out lip, out beyondLip);

                if (!climbHauled && footing == Footing.Climbing && taut && !hook && reeledIn && _config.ClimbHaulForce.Value > 0f)
                {
                    local.DragTowards(local.Center + span.Direction.ToUnity(), _config.ClimbHaulForce.Value);
                    climbHauled = true;
                }
            }

            _reelWatch.End();

            foreach (RopeLink link in _snapped)
                _registry.Untie(link);

            bool canAssist = _config.LedgePush.Value > 0f && (footing == Footing.Airborne || footing == Footing.Unconscious);
            total = _ledge.Adjust(total.ToNumerics(), local.Center.ToNumerics(), local.data.avarageVelocity.ToNumerics(), canAssist, _config.LedgePush.Value, Time.fixedDeltaTime, _obstacles).ToUnity();
            bool canMantle = (hauled || (_mantle.IsPushing && partnerHolds)) && footing != Footing.Grounded;
            total += _mantle.Push(local.Center.ToNumerics(), local.data.avarageVelocity.ToNumerics(), lip, beyondLip, canMantle, _config.MantleStep.Value, _config.MantlePush.Value, Time.fixedDeltaTime, _obstacles).ToUnity();

            if (total.sqrMagnitude > 0f)
                local.AddForce(total);

            KeepHangingLight(local, total);

            if (partnerHolds)
                Belay(local);
        }

        public void ApplyToDummy(TestDummy dummy)
        {
            if (!dummy.IsPlaced)
                return;

            float stability = _stability.Resolve(TestDummy.Actor, _registry.Links);
            Vector3 total = Vector3.zero;

            foreach (RopeLink link in _registry.Links)
            {
                if (link.Contains(TestDummy.Actor))
                    total += _tension.Solve(_ropes.SpanFor(link, TestDummy.Actor), dummy.Velocity.ToNumerics(), _lengths.EffectiveLength(link), stability).ToUnity();
            }

            if (total.sqrMagnitude > 0f)
                dummy.Push(total);
        }

        private bool TryGetLip(RopeLink link, int actor, NumericsVector3 partnerPoint, float floorBelowPartner, NumericsVector3 center, out NumericsVector3 lip, out NumericsVector3 beyondLip)
        {
            lip = NumericsVector3.Zero;
            beyondLip = NumericsVector3.Zero;

            if (!_ropes.TryGetPath(link, out RopeWrapPath path))
                return false;

            if (path.ContactCount == 0)
            {
                lip = partnerPoint - NumericsVector3.UnitY * floorBelowPartner;
                beyondLip = lip + (partnerPoint - center);
                return true;
            }

            IReadOnlyList<NumericsVector3> points = path.Points;
            bool fromStart = actor == link.FirstActor;
            lip = fromStart ? points[1] : points[points.Count - 2];
            beyondLip = fromStart ? points[2] : points[points.Count - 3];
            return true;
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
