using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Debugging
{
    public sealed class DebugTools : IDebugTools
    {
        private readonly RepkaConfig _config;
        private readonly RopeDebugReport _report;
        private readonly RopeDebugOverlay _overlay;
        private readonly KeyBinding _dummyKey;
        private readonly TestDummyInput _dummyInput;

        public DebugTools(RepkaConfig config, RopeDebugReport report, RopeDebugOverlay overlay, KeyBinding dummyKey, TestDummyInput dummyInput)
        {
            _config = config;
            _report = report;
            _overlay = overlay;
            _dummyKey = dummyKey;
            _dummyInput = dummyInput;
        }

        public void TickInput(int actor, Transform view)
        {
            _dummyInput.Tick(actor, view);
        }

        public void ShowOverlay(Character local, int actor)
        {
            if (_config.ShowDebugOverlay.Value)
                _overlay.Show(_report.Build(local, actor));
            else
                _overlay.Hide();
        }

        public void Hide()
        {
            _overlay.Hide();
        }

        public void Dispose()
        {
            _dummyKey.Dispose();
        }
    }
}
