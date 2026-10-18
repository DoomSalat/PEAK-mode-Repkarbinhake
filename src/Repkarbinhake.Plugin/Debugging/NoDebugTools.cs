using UnityEngine;

namespace Repkarbinhake.Plugin.Debugging
{
    public sealed class NoDebugTools : IDebugTools
    {
        public void TickInput(int actor, Transform view)
        {
        }

        public void ShowOverlay(Character local, int actor)
        {
        }

        public void Hide()
        {
        }

        public void Dispose()
        {
        }
    }
}
