using System;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;

namespace Repkarbinhake.Core.Physics
{
    public sealed class RopeReel
    {
        public const float MinReelInFactor = 0.15f;
        public const float AirborneReelOutFactor = 0.3f;

        private readonly IRopeSettings _settings;
        private readonly RopeHandPolicy _policy;

        public RopeReel(IRopeSettings settings, RopeHandPolicy policy)
        {
            _settings = settings;
            _policy = policy;
        }

        public float FullLength => _settings.MaxLength;

        public float EffectiveLength(RopeLink link)
        {
            return Math.Clamp(_settings.MaxLength - link.TotalReel, _settings.MinLength, _settings.MaxLength);
        }

        public ReelStep StepReel(RopeLink link, int actor, float scrollInput, float deltaTime, float load, Footing footing, float stamina)
        {
            float current = link.EndOf(actor).Reel;

            if (scrollInput >= 0f)
                return ReelOut(current, scrollInput, deltaTime, footing);

            if (!_policy.CanReelIn(footing))
                return new ReelStep(current, 0f);

            float wanted = -scrollInput * _settings.ReelSpeed * ReelInFactor(load) * deltaTime;
            float costPerMeter = StaminaPerMeter(load);

            if (costPerMeter > 0f)
                wanted = Math.Min(wanted, Math.Max(stamina, 0f) / costPerMeter);

            float next = Math.Clamp(current + wanted, 0f, MaxReel(link, actor));
            float reeled = Math.Max(next - current, 0f);
            return new ReelStep(next, reeled * costPerMeter);
        }

        public float ReelForLength(RopeLink link, int actor, float length)
        {
            float otherReel = link.EndOf(link.Partner(actor)).HeldReel;
            return Math.Clamp(_settings.MaxLength - otherReel - length, 0f, MaxReel(link, actor));
        }

        public float ReelInFactor(float load)
        {
            if (load <= 0f)
                return 1f;

            if (_settings.ArmStrength <= 0f)
                return 0f;

            return Math.Clamp(1f - load / _settings.ArmStrength, MinReelInFactor, 1f);
        }

        public float StaminaPerMeter(float load)
        {
            if (load <= 0f)
                return 0f;

            float loadShare = _settings.ArmStrength <= 0f ? 1f : Math.Min(load / _settings.ArmStrength, 1f);
            return _settings.ReelStaminaPerMeter * loadShare;
        }

        private ReelStep ReelOut(float current, float scrollInput, float deltaTime, Footing footing)
        {
            if (!_policy.CanReelOut(footing))
                return new ReelStep(current, 0f);

            float speed = footing == Footing.Airborne ? _settings.ReelSpeed * AirborneReelOutFactor : _settings.ReelSpeed;
            return new ReelStep(Math.Max(current - scrollInput * speed * deltaTime, 0f), 0f);
        }

        private float MaxReel(RopeLink link, int actor)
        {
            float otherReel = link.EndOf(link.Partner(actor)).HeldReel;
            return Math.Max(_settings.MaxLength - _settings.MinLength - otherReel, 0f);
        }
    }
}
