using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class AnchorRopeSettings : IRopeSettings
    {
        private readonly IRopeSettings _rope;
        private readonly RepkaConfig _config;

        public AnchorRopeSettings(IRopeSettings rope, RepkaConfig config)
        {
            _rope = rope;
            _config = config;
        }

        public float MinLength => _rope.MinLength;
        public float MaxLength => _rope.MaxLength;
        public float ReelSpeed => _rope.ReelSpeed;
        public float ArmStrength => _rope.ArmStrength;
        public float ReelStaminaPerMeter => _rope.ReelStaminaPerMeter;
        public float Stiffness => _config.AnchorStiffness.Value;
        public float Damping => _config.AnchorDamping.Value;
        public float MaxPull => _config.AnchorMaxPull.Value;
        public float SnapDistance => _rope.SnapDistance;
        public float BraceResistance => _rope.BraceResistance;
        public float GroupBonus => _rope.GroupBonus;
        public float ChainSupport => _rope.ChainSupport;
        public float MaxStability => _rope.MaxStability;
        public float TieHoldSeconds => _rope.TieHoldSeconds;
        public float UntieHoldSeconds => _rope.UntieHoldSeconds;
    }
}
