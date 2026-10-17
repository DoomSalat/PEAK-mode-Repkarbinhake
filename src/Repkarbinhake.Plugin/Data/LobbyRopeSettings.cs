using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class LobbyRopeSettings : IRopeSettings
    {
        private readonly RepkaConfig _local;
        private float _hostMaxLength = -1f;

        public LobbyRopeSettings(RepkaConfig local)
        {
            _local = local;
        }

        public float MinLength => _local.MinLength;
        public float MaxLength => _hostMaxLength > 0f ? _hostMaxLength : _local.MaxLength;
        public float ReelSpeed => _local.ReelSpeed;
        public float ArmStrength => _local.ArmStrength;
        public float ReelStaminaPerMeter => _local.ReelStaminaPerMeter;
        public float Stiffness => _local.Stiffness;
        public float Damping => _local.Damping;
        public float MaxPull => _local.MaxPull;
        public float SnapDistance => _local.SnapDistance;
        public float BraceResistance => _local.BraceResistance;
        public float GroupBonus => _local.GroupBonus;
        public float ChainSupport => _local.ChainSupport;
        public float MaxStability => _local.MaxStability;
        public float TieHoldSeconds => _local.TieHoldSeconds;
        public float UntieHoldSeconds => _local.UntieHoldSeconds;

        public void ApplyHostMaxLength(float maxLength)
        {
            _hostMaxLength = maxLength;
        }

        public void ForgetHost()
        {
            _hostMaxLength = -1f;
        }
    }
}
