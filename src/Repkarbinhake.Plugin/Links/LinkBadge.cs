using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public readonly struct LinkBadge
    {
        public LinkBadge(Color color, int number)
        {
            Color = color;
            Number = number;
        }

        public Color Color { get; }
        public int Number { get; }
    }
}
