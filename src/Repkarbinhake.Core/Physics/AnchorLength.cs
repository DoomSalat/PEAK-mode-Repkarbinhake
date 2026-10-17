using System;

namespace Repkarbinhake.Core.Physics
{
    public sealed class AnchorLength
    {
        public const float Slack = 0.2f;

        private bool _locked;
        private float _length = -1f;

        public bool IsLocked => _locked;

        public float Next(float pathLength, bool supported, bool falling, float minLength, float maxLength)
        {
            if (!_locked && falling && _length >= 0f)
                _locked = true;

            if (_locked && supported)
                _locked = false;

            if (!_locked)
                _length = Math.Clamp(pathLength + Slack, minLength, maxLength);

            return _length;
        }

        public void Reset()
        {
            _locked = false;
            _length = -1f;
        }
    }
}
