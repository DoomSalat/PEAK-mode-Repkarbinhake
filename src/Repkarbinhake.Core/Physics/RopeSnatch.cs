using System;
using System.Collections.Generic;

namespace Repkarbinhake.Core.Physics
{
    public sealed class RopeSnatch
    {
        public const float MaxStepGrowth = 2f;

        private readonly Dictionary<string, float> _lastStretch = new Dictionary<string, float>();

        public float Measure(string linkKey, float stretch, float ownSeparatingSpeed, float deltaTime)
        {
            bool known = _lastStretch.TryGetValue(linkKey, out float last);
            _lastStretch[linkKey] = stretch;

            if (!known || stretch <= 0f || deltaTime <= 0f)
                return 0f;

            float step = stretch - MathF.Max(last, 0f);

            if (step > MaxStepGrowth)
                return 0f;

            float growth = step / deltaTime;
            return MathF.Max(growth - ownSeparatingSpeed, 0f);
        }

        public void Forget(string linkKey)
        {
            _lastStretch.Remove(linkKey);
        }

        public void Clear()
        {
            _lastStretch.Clear();
        }
    }
}
