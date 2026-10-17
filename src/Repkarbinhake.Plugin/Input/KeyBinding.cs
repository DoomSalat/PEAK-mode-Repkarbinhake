using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class KeyBinding
    {
        private readonly ConfigEntry<string> _path;
        private InputAction _action;

        public KeyBinding(ConfigEntry<string> path)
        {
            _path = path;
            Rebuild();
            _path.SettingChanged += (_, __) => Rebuild();
        }

        public bool IsPressed => _action.IsPressed();
        public bool WasPressed => _action.WasPressedThisFrame();
        public bool WasReleased => _action.WasReleasedThisFrame();

        public void Dispose()
        {
            _action.Disable();
            _action.Dispose();
        }

        private void Rebuild()
        {
            _action?.Disable();
            _action?.Dispose();
            _action = new InputAction(binding: _path.Value);
            _action.Enable();
        }
    }
}
