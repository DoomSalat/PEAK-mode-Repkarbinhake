namespace Repkarbinhake.Core.Data
{
    public interface IRopeSettings
    {
        float MinLength { get; }
        float MaxLength { get; }
        float ReelSpeed { get; }
        float ArmStrength { get; }
        float ReelStaminaPerMeter { get; }
        float Stiffness { get; }
        float Damping { get; }
        float MaxPull { get; }
        float SnapDistance { get; }
        float BraceResistance { get; }
        float GroupBonus { get; }
        float ChainSupport { get; }
        float MaxStability { get; }
        float TieHoldSeconds { get; }
        float UntieHoldSeconds { get; }
    }
}
