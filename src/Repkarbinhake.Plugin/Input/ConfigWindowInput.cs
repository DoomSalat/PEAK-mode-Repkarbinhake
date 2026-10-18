using Repkarbinhake.Plugin.Patches;
using Repkarbinhake.Plugin.Visual;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class ConfigWindowInput
    {
        private readonly ConfigEditorWindow _window;
        private readonly KeyBinding _key;

        public ConfigWindowInput(ConfigEditorWindow window, KeyBinding key)
        {
            _window = window;
            _key = key;
        }

        public bool IsOpen => _window.IsOpen;

        public void Tick()
        {
            if (_key.WasPressed)
                _window.Toggle();

            RepkaHooks.ConfigWindowOpen = _window.IsOpen;
        }

        public void Dispose()
        {
            _key.Dispose();
        }
    }
}
