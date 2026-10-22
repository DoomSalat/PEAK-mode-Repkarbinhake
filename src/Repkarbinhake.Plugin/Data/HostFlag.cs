using System;
using BepInEx.Configuration;
using Photon.Pun;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class HostFlag
    {
        private readonly bool _fallback;
        private bool? _host;

        public HostFlag(string key, ConfigEntry<bool> local, bool fallback)
        {
            Key = key;
            Local = local;
            _fallback = fallback;
        }

        public event Action<bool> Changed;

        public string Key { get; }
        public ConfigEntry<bool> Local { get; }

        public bool Value
        {
            get
            {
                if (PhotonNetwork.IsMasterClient)
                    return Local.Value;

                return _host ?? _fallback;
            }
        }

        public void ApplyHost(bool value)
        {
            bool before = Value;
            _host = value;

            if (Value != before)
                Changed?.Invoke(Value);
        }

        public void ApplyLocal()
        {
            if (PhotonNetwork.IsMasterClient)
                Changed?.Invoke(Value);
        }

        public void ForgetHost()
        {
            _host = null;
        }
    }
}
