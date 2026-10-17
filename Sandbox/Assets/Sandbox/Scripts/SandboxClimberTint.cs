using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxClimberTint
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color"); // Static: shader property id is global

        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private readonly SandboxClimberColors _colors;

        public SandboxClimberTint(SandboxClimberColors colors)
        {
            _colors = colors;
        }

        public void Apply(SandboxClimber climber, bool isSelected, bool isHolding)
        {
            _block.SetColor(ColorProperty, ColorOf(climber, isSelected, isHolding));
            climber.BodyRenderer.SetPropertyBlock(_block);
        }

        private Color ColorOf(SandboxClimber climber, bool isSelected, bool isHolding)
        {
            if (isSelected)
                return _colors.Selected;

            if (climber.IsDead)
                return _colors.Dead;

            if (climber.IsUnconscious)
                return _colors.Unconscious;

            return isHolding ? _colors.Holding : _colors.Idle;
        }
    }
}
