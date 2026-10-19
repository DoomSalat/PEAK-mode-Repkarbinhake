using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class AutoHandUp
    {
        private const float StartRange = 3.5f;
        private const float KeepRange = 4f;
        private const float MaxLookAngle = 60f;
        private const float StandSeconds = 0.3f;
        private const float ArriveRange = 1.5f;
        private const float MaxSeconds = 3f;

        private readonly LinkRegistry _registry;
        private readonly CharacterDirectory _characters;
        private readonly ActorStates _actors;
        private readonly ConfigEntry<bool> _enabled;
        private float _activeFor;
        private float _standingFor;

        public AutoHandUp(LinkRegistry registry, CharacterDirectory characters, ActorStates actors, ConfigEntry<bool> enabled)
        {
            _registry = registry;
            _characters = characters;
            _actors = actors;
            _enabled = enabled;
        }

        public int Target { get; private set; }

        public void Tick(Character local, int actor, bool reelingIn, float deltaTime)
        {
            if (!_enabled.Value)
            {
                Stop();
                return;
            }

            if (Target != 0)
            {
                Keep(local, deltaTime);
                return;
            }

            if (reelingIn && _registry.TryGetHeld(actor, out RopeLink link))
                TryStart(local, actor, link);
        }

        public void Stop()
        {
            Target = 0;
            _activeFor = 0f;
            _standingFor = 0f;
        }

        public static bool IsHelpless(Footing footing) // Static: pure rule shared with the hand-up patch feed
        {
            return footing == Footing.Airborne || footing == Footing.Unconscious;
        }

        private void TryStart(Character local, int actor, RopeLink link)
        {
            int partner = link.Partner(actor);

            Footing footing = _actors.FootingOf(partner);

            if (partner <= 0 || !(IsHelpless(footing) || footing == Footing.Climbing) || _actors.IsDead(partner) || !_characters.TryFind(partner, out Character target))
                return;

            if (!CanReach(local, target, StartRange))
                return;

            _registry.Release(link);
            Target = partner;
            _activeFor = 0f;
            _standingFor = 0f;
            local.data.sincePressReach = 0f;
        }

        private void Keep(Character local, float deltaTime)
        {
            _activeFor += deltaTime;

            if (_activeFor > MaxSeconds || _actors.IsDead(Target) || !_characters.TryFind(Target, out Character target) || !CanReach(local, target, KeepRange))
            {
                Stop();
                return;
            }

            Footing footing = _actors.FootingOf(Target);
            bool settled = footing == Footing.Grounded || (footing == Footing.Unconscious && (target.Center - local.Center).sqrMagnitude < ArriveRange * ArriveRange);
            _standingFor = settled ? _standingFor + deltaTime : 0f;

            if (_standingFor >= StandSeconds)
            {
                Stop();
                return;
            }

            local.data.sincePressReach = 0f;
        }

        private bool CanReach(Character local, Character target, float range)
        {
            Vector3 toTarget = target.Center - local.Center;

            if (toTarget.sqrMagnitude > range * range || Vector3.Angle(local.data.lookDirection, toTarget) > MaxLookAngle)
                return false;

            return local.data.currentItem == null && !local.data.isClimbingAnything;
        }
    }
}
