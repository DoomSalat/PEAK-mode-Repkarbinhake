using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Patches;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Debugging
{
    public sealed class DebugTools : IDebugTools
    {
        private readonly RepkaConfig _config;
        private readonly RopeDebugReport _report;
        private readonly RopeDebugOverlay _overlay;
        private readonly ConfigEditorWindow _window;
        private readonly KeyBinding _windowKey;
        private readonly KeyBinding _dummyKey;
        private readonly TestDummyInput _dummyInput;

        public DebugTools(RepkaConfig config, RopeDebugReport report, RopeDebugOverlay overlay, ConfigEditorWindow window, KeyBinding windowKey, KeyBinding dummyKey, TestDummyInput dummyInput)
        {
            _config = config;
            _report = report;
            _overlay = overlay;
            _window = window;
            _windowKey = windowKey;
            _dummyKey = dummyKey;
            _dummyInput = dummyInput;
        }

        public bool BlocksInput => _window.IsOpen;

        public void TickWindow()
        {
            if (_windowKey.WasPressed)
                _window.Toggle();

            RepkaHooks.ConfigWindowOpen = _window.IsOpen;
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
            _windowKey.Dispose();
            _dummyKey.Dispose();
        }
    }
}
