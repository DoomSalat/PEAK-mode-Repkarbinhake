using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class HookLengths
    {
        private readonly Dictionary<string, float> _lengths = new Dictionary<string, float>();

        public void Set(RopeLink link, float length)
        {
            _lengths[link.Key] = length;
        }

        public void Clear()
        {
            _lengths.Clear();
        }

        public bool TryGet(RopeLink link, out float length)
        {
            return _lengths.TryGetValue(link.Key, out length);
        }
    }
}
