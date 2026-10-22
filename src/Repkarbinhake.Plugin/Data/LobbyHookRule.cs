using BepInEx.Configuration;
using Photon.Pun;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class LobbyHookRule
    {
        private readonly ConfigEntry<bool> _local;
        private bool? _host;

        public LobbyHookRule(ConfigEntry<bool> local)
        {
            _local = local;
        }

        public bool Allowed
        {
            get
            {
                if (PhotonNetwork.IsMasterClient)
                    return _local.Value;

                return !_host.HasValue || _host.Value;
            }
        }

        public void ApplyHost(bool allowed)
        {
            _host = allowed;
        }

        public void ForgetHost()
        {
            _host = null;
        }
    }
}
