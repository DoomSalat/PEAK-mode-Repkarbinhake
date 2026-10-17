using UnityEngine;

namespace Repkarbinhake.Plugin.Debugging
{
    public interface IDebugTools
    {
        bool BlocksInput { get; }

        void TickWindow();
        void TickInput(int actor, Transform view);
        void ShowOverlay(Character local, int actor);
        void Hide();
        void Dispose();
    }
}
