using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class RoomLinkSync : PhotonRoomCallbacks
    {
        private const float HostChoresInterval = 2f;
        private const float SweepMaxAge = 30f;

        private readonly PhotonLinkTransport _transport;
        private readonly LinkRegistry _registry;
        private readonly UntiedKeySweeper _sweeper;
        private readonly DepartedUntieRule _departedRule;
        private readonly LobbyRopeSettings _settings;
        private readonly LobbyHookRule _hookRule;
        private readonly RepkaConfig _config;
        private readonly HashSet<int> _present = new HashSet<int>();
        private readonly List<RopeLink> _departed = new List<RopeLink>();
        private readonly List<LinkUpdate> _stale = new List<LinkUpdate>();
        private float _hostTimer;
        private float _now;

        public RoomLinkSync(PhotonLinkTransport transport, LinkRegistry registry, UntiedKeySweeper sweeper, DepartedUntieRule departedRule, LobbyRopeSettings settings, LobbyHookRule hookRule, RepkaConfig config)
        {
            _transport = transport;
            _registry = registry;
            _sweeper = sweeper;
            _departedRule = departedRule;
            _settings = settings;
            _hookRule = hookRule;
            _config = config;
        }

        public bool IsReadingSnapshot { get; private set; }
        public bool IsHost => PhotonNetwork.IsMasterClient;

        public override void Enable()
        {
            _config.MaxLengthEntry.SettingChanged += OnMaxLengthChanged;
            _config.AllowHooks.SettingChanged += OnAllowHooksChanged;
            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();

            if (_config.MaxLengthEntry != null)
                _config.MaxLengthEntry.SettingChanged -= OnMaxLengthChanged;

            if (_config.AllowHooks != null)
                _config.AllowHooks.SettingChanged -= OnAllowHooksChanged;
        }

        public void Tick(float deltaTime, float now)
        {
            _now = now;

            if (!PhotonNetwork.InRoom || !IsHost)
                return;

            _hostTimer += deltaTime;

            if (_hostTimer < HostChoresInterval)
                return;

            _hostTimer = 0f;
            UntieDeparted();
            SweepUntied();
        }

        public override void OnJoinedRoom()
        {
            _registry.Clear();
            _sweeper.Clear();
            _settings.ForgetHost();
            _hookRule.ForgetHost();
            _registry.Bind(PhotonNetwork.LocalPlayer.ActorNumber);
            IsReadingSnapshot = true;
            ReadProperties(PhotonNetwork.CurrentRoom.CustomProperties);
            IsReadingSnapshot = false;

            if (IsHost)
                TakeHostDuties();
        }

        public override void OnLeftRoom()
        {
            _registry.Clear();
            _sweeper.Clear();
            _settings.ForgetHost();
            _hookRule.ForgetHost();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            ReadProperties(propertiesThatChanged);
        }

        public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            if (IsHost)
                UntieDeparted();
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (IsHost)
                TakeHostDuties();
        }

        private void ReadProperties(Hashtable properties)
        {
            _transport.Dispatch(properties, _registry, _sweeper, _now);

            if (_transport.TryReadMaxLength(properties, out float maxLength))
                _settings.ApplyHostMaxLength(maxLength);

            if (_transport.TryReadAllowHooks(properties, out bool allowHooks))
                _hookRule.ApplyHost(allowHooks);
        }

        private void TakeHostDuties()
        {
            _transport.PublishMaxLength(_config.MaxLength);
            _transport.PublishAllowHooks(_config.AllowHooks.Value);
            UntieDeparted();
        }

        private void OnMaxLengthChanged(object sender, EventArgs arguments)
        {
            if (PhotonNetwork.InRoom && IsHost)
                _transport.PublishMaxLength(_config.MaxLength);
        }

        private void OnAllowHooksChanged(object sender, EventArgs arguments)
        {
            if (PhotonNetwork.InRoom && IsHost)
                _transport.PublishAllowHooks(_config.AllowHooks.Value);
        }

        private void UntieDeparted()
        {
            _present.Clear();

            foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
            {
                if (player.IsInactive)
                    continue;

                _present.Add(player.ActorNumber);
                _present.Add(AnchorActors.Of(player.ActorNumber));
            }

            foreach (RopeLink link in _registry.Links)
            {
                KeepLocal(link.FirstActor);
                KeepLocal(link.SecondActor);
            }

            _departedRule.Collect(_registry.Links, _present, _departed);

            foreach (RopeLink link in _departed)
                _registry.Untie(link);
        }

        private void KeepLocal(int actor)
        {
            if (actor < 0 && !AnchorActors.IsAnchor(actor))
                _present.Add(actor);
        }

        private void SweepUntied()
        {
            _sweeper.Collect(_now, SweepMaxAge, _stale);

            foreach (LinkUpdate update in _stale)
                _transport.DeleteIfUnchanged(update);
        }
    }
}
