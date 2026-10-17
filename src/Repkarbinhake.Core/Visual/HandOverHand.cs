using System;

namespace Repkarbinhake.Core.Visual
{
    public sealed class HandOverHand
    {
        private const float HalfCycle = 0.5f;
        private const float FullTurn = MathF.PI * 2f;

        private readonly float _strokeLength;
        private readonly float _near;
        private readonly float _far;

        public HandOverHand(float strokeLength, float near, float far)
        {
            _strokeLength = strokeLength;
            _near = near;
            _far = far;
        }

        public float Phase { get; private set; }
        public float RightReach => ReachAt(Phase);
        public float LeftReach => ReachAt((Phase + HalfCycle) % 1f);

        public void Advance(float reeledMeters)
        {
            if (_strokeLength <= 0f)
                return;

            float phase = (Phase + reeledMeters / _strokeLength) % 1f;
            Phase = phase < 0f ? phase + 1f : phase;
        }

        private float ReachAt(float phase)
        {
            float pulledIn = (1f - MathF.Cos(phase * FullTurn)) * HalfCycle;
            return _far + (_near - _far) * pulledIn;
        }
    }
}
