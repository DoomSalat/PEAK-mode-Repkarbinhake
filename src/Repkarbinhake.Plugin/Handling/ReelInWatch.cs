using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class ReelInWatch
    {
        private const float HoldSeconds = 0.5f;
        private const float MinStep = 0.001f;

        private readonly Dictionary<string, float> _lastReel = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _pullingFor = new Dictionary<string, float>();
        private readonly HashSet<string> _seen = new HashSet<string>();
        private readonly List<string> _stale = new List<string>();

        public void Begin()
        {
            _seen.Clear();
        }

        public bool IsReelingIn(RopeLink link, int partner, float deltaTime)
        {
            string key = link.Key;
            float reel = link.EndOf(partner).HeldReel;
            _seen.Add(key);
            float hold = _pullingFor.TryGetValue(key, out float left) ? left - deltaTime : 0f;

            if (_lastReel.TryGetValue(key, out float last) && reel > last + MinStep)
                hold = HoldSeconds;

            _lastReel[key] = reel;
            _pullingFor[key] = hold;
            return hold > 0f;
        }

        public void End()
        {
            _stale.Clear();

            foreach (string key in _lastReel.Keys)
            {
                if (!_seen.Contains(key))
                    _stale.Add(key);
            }

            foreach (string key in _stale)
            {
                _lastReel.Remove(key);
                _pullingFor.Remove(key);
            }
        }
    }
}
