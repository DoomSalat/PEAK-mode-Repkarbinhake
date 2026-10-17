using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Tests.Fakes
{
    public sealed class TestSettings : IRopeSettings
    {
        public float MinLength { get; set; } = 1.5f;
        public float MaxLength { get; set; } = 14f;
        public float ReelSpeed { get; set; } = 6f;
        public float ArmStrength { get; set; } = 12f;
        public float ReelStaminaPerMeter { get; set; } = 0.1f;
        public float Stiffness { get; set; } = 30f;
        public float Damping { get; set; } = 6f;
        public float MaxPull { get; set; } = 70f;
        public float SnapDistance { get; set; } = 60f;
        public float BraceResistance { get; set; } = 0.8f;
        public float GroupBonus { get; set; } = 0.25f;
        public float ChainSupport { get; set; } = 0.6f;
        public float MaxStability { get; set; } = 0.97f;
        public float TieHoldSeconds { get; set; } = 0.8f;
        public float UntieHoldSeconds { get; set; } = 1.2f;
    }
}
