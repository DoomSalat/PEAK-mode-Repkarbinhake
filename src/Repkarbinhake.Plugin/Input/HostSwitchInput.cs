using BepInEx.Configuration;
using Photon.Pun;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class HostSwitchInput
    {
        private readonly KeyBinding _key;
        private readonly ConfigEntry<bool> _switch;

        public HostSwitchInput(KeyBinding key, ConfigEntry<bool> hostSwitch)
        {
            _key = key;
            _switch = hostSwitch;
        }

        public void Tick()
        {
            if (_key.WasPressed && PhotonNetwork.IsMasterClient)
                _switch.Value = !_switch.Value;
        }

        public void Dispose()
        {
            _key.Dispose();
        }
    }
}
