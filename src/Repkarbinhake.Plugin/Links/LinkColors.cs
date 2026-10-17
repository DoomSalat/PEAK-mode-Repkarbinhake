using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class LinkColors
    {
        private readonly LinkRegistry _registry;
        private readonly Dictionary<string, int> _slots = new Dictionary<string, int>();
        private readonly HashSet<int> _used = new HashSet<int>();

        public LinkColors(LinkRegistry registry)
        {
            _registry = registry;
        }

        public void Assign(RopeLink link)
        {
            _used.Clear();

            foreach (RopeLink other in _registry.Links)
            {
                if (other.Key != link.Key && _slots.TryGetValue(other.Key, out int slot))
                    _used.Add(slot);
            }

            int free = 0;

            while (free < RepkaColors.LinkPalette.Length && _used.Contains(free))
                free++;

            _slots[link.Key] = free < RepkaColors.LinkPalette.Length ? free : _used.Count % RepkaColors.LinkPalette.Length;
        }

        public void Release(RopeLink link)
        {
            _slots.Remove(link.Key);
        }

        public void CollectFor(int actor, List<Color> colors)
        {
            colors.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                if (link.Contains(actor) && !AnchorActors.IsAnchor(link.FirstActor) && _slots.TryGetValue(link.Key, out int slot))
                    colors.Add(RepkaColors.LinkPalette[slot]);
            }
        }
    }
}
