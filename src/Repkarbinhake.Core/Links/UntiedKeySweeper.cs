using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class UntiedKeySweeper
    {
        private readonly Dictionary<string, UntiedEntry> _untied = new Dictionary<string, UntiedEntry>();
        private readonly List<string> _expiredKeys = new List<string>();

        public int TrackedCount => _untied.Count;

        public void Observe(LinkUpdate update, float now)
        {
            if (update.State.IsTied)
            {
                _untied.Remove(update.Key);
                return;
            }

            _untied[update.Key] = new UntiedEntry(update, now);
        }

        public void Forget(string key)
        {
            _untied.Remove(key);
        }

        public void Collect(float now, float maxAge, List<LinkUpdate> result)
        {
            result.Clear();
            _expiredKeys.Clear();

            foreach (KeyValuePair<string, UntiedEntry> pair in _untied)
            {
                if (now - pair.Value.ObservedAt < maxAge)
                    continue;

                result.Add(pair.Value.Update);
                _expiredKeys.Add(pair.Key);
            }

            foreach (string key in _expiredKeys)
                _untied.Remove(key);
        }

        public void Clear()
        {
            _untied.Clear();
        }

        private readonly struct UntiedEntry
        {
            public UntiedEntry(LinkUpdate update, float observedAt)
            {
                Update = update;
                ObservedAt = observedAt;
            }

            public LinkUpdate Update { get; }
            public float ObservedAt { get; }
        }
    }
}
