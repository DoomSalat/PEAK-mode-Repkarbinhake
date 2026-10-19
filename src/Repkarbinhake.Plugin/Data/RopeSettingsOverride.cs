using BepInEx.Configuration;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class RopeSettingsOverride : IRopeSettings
    {
        private readonly IRopeSettings _rope;
        private readonly ConfigEntry<float> _stiffness;
        private readonly ConfigEntry<float> _damping;
        private readonly ConfigEntry<float> _maxPull;

        public RopeSettingsOverride(IRopeSettings rope, ConfigEntry<float> stiffness, ConfigEntry<float> damping, ConfigEntry<float> maxPull)
        {
            _rope = rope;
            _stiffness = stiffness;
            _damping = damping;
            _maxPull = maxPull;
        }

        public float MinLength => _rope.MinLength;
        public float MaxLength => _rope.MaxLength;
        public float ReelSpeed => _rope.ReelSpeed;
        public float ArmStrength => _rope.ArmStrength;
        public float ReelStaminaPerMeter => _rope.ReelStaminaPerMeter;
        public float Stiffness => _stiffness == null ? _rope.Stiffness : _stiffness.Value;
        public float Damping => _damping == null ? _rope.Damping : _damping.Value;
        public float MaxPull => _maxPull == null ? _rope.MaxPull : _maxPull.Value;
        public float SnapDistance => _rope.SnapDistance;
        public float BraceResistance => _rope.BraceResistance;
        public float GroupBonus => _rope.GroupBonus;
        public float ChainSupport => _rope.ChainSupport;
        public float MaxStability => _rope.MaxStability;
        public float TieHoldSeconds => _rope.TieHoldSeconds;
        public float UntieHoldSeconds => _rope.UntieHoldSeconds;
    }
}
