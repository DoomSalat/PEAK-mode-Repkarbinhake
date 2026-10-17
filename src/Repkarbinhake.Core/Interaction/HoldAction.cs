using System;

namespace Repkarbinhake.Core.Interaction
{
    public sealed class HoldAction
    {
        public const int NoTarget = 0;

        private float _elapsed;
        private float _duration;

        public int Target { get; private set; } = NoTarget;
        public bool IsActive => Target != NoTarget;
        public float Progress => IsActive && _duration > 0f ? Math.Min(_elapsed / _duration, 1f) : 0f;

        public void Begin(int target, float duration)
        {
            Target = target;
            _duration = duration;
            _elapsed = 0f;
        }

        public void Cancel()
        {
            Target = NoTarget;
            _elapsed = 0f;
        }

        public bool Advance(bool isHeld, int currentTarget, float deltaTime)
        {
            if (!IsActive)
                return false;

            if (!isHeld || currentTarget != Target)
            {
                Cancel();
                return false;
            }

            _elapsed += deltaTime;

            if (_elapsed < _duration)
                return false;

            Cancel();
            return true;
        }
    }
}
