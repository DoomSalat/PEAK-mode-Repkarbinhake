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
    public sealed class RoomLinkSync : IInRoomCallbacks, IMatchmakingCallbacks
    {
        private const float HostChoresInterval = 2f;
        private const float SweepMaxAge = 30f;

        private readonly PhotonLinkTransport _transport;
        private readonly LinkRegistry _registry;
        private readonly UntiedKeySweeper _sweeper;
        private readonly DepartedUntieRule _departedRule;
        private readonly LobbyRopeSettings _settings;
        private readonly RepkaConfig _config;
        private readonly TestAnchor _anchor;
        private readonly TestDummy _dummy;
        private readonly HashSet<int> _present = new HashSet<int>();
        private readonly List<RopeLink> _departed = new List<RopeLink>();
        private readonly List<LinkUpdate> _stale = new List<LinkUpdate>();
        private float _hostTimer;
        private float _now;

        public RoomLinkSync(PhotonLinkTransport transport, LinkRegistry registry, UntiedKeySweeper sweeper, DepartedUntieRule departedRule, LobbyRopeSettings settings, RepkaConfig config, TestAnchor anchor, TestDummy dummy)
        {
            _transport = transport;
            _registry = registry;
            _sweeper = sweeper;
            _departedRule = departedRule;
            _settings = settings;
            _config = config;
            _anchor = anchor;
            _dummy = dummy;
        }

        public event Action JoinedRoom;
        public event Action LeftRoom;

        public bool IsReadingSnapshot { get; private set; }
        public bool IsHost => PhotonNetwork.IsMasterClient;

        public void Enable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            _config.MaxLengthEntry.SettingChanged += OnMaxLengthChanged;

            if (PhotonNetwork.InRoom)
                OnJoinedRoom();
        }

        public void Disable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);

            if (_config.MaxLengthEntry != null)
                _config.MaxLengthEntry.SettingChanged -= OnMaxLengthChanged;
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

        public void OnJoinedRoom()
        {
            _registry.Clear();
            _sweeper.Clear();
            _settings.ForgetHost();
            _registry.Bind(PhotonNetwork.LocalPlayer.ActorNumber);
            IsReadingSnapshot = true;
            ReadProperties(PhotonNetwork.CurrentRoom.CustomProperties);
            IsReadingSnapshot = false;
            JoinedRoom?.Invoke();

            if (IsHost)
                TakeHostDuties();
        }

        public void OnLeftRoom()
        {
            _anchor.Remove();

            if (_dummy.IsPlaced)
                _dummy.Remove();

            _registry.Clear();
            _sweeper.Clear();
            _settings.ForgetHost();
            LeftRoom?.Invoke();
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            ReadProperties(propertiesThatChanged);
        }

        public void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            if (IsHost)
                UntieDeparted();
        }

        public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (IsHost)
                TakeHostDuties();
        }

        public void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
        }

        public void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
        {
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        private void ReadProperties(Hashtable properties)
        {
            _transport.Dispatch(properties, _registry, _sweeper, _now);

            if (_transport.TryReadMaxLength(properties, out float maxLength))
                _settings.ApplyHostMaxLength(maxLength);
        }

        private void TakeHostDuties()
        {
            _transport.PublishMaxLength(_config.MaxLength);
            UntieDeparted();
        }

        private void OnMaxLengthChanged(object sender, EventArgs arguments)
        {
            if (PhotonNetwork.InRoom && IsHost)
                _transport.PublishMaxLength(_config.MaxLength);
        }

        private void UntieDeparted()
        {
            _present.Clear();

            foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
            {
                if (!player.IsInactive)
                    _present.Add(player.ActorNumber);
            }

            if (_anchor.IsPlaced)
                _present.Add(TestAnchor.Actor);

            if (_dummy.IsPlaced)
                _present.Add(TestDummy.Actor);

            _departedRule.Collect(_registry.Links, _present, _departed);

            foreach (RopeLink link in _departed)
                _registry.Untie(link);
        }

        private void SweepUntied()
        {
            _sweeper.Collect(_now, SweepMaxAge, _stale);

            foreach (LinkUpdate update in _stale)
                _transport.DeleteIfUnchanged(update);
        }
    }
}
