using System.Collections.Generic;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class ModVersionCheck : IInRoomCallbacks, IMatchmakingCallbacks
    {
        private const string VersionKey = "rkb.ver";

        private readonly string _version;
        private readonly ManualLogSource _log;
        private readonly Dictionary<int, string> _reported = new Dictionary<int, string>();

        public ModVersionCheck(string version, ManualLogSource log)
        {
            _version = version;
            _log = log;
        }

        public void Enable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (PhotonNetwork.InRoom)
                OnJoinedRoom();
        }

        public void Disable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnJoinedRoom()
        {
            _reported.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { VersionKey, _version } });

            foreach (PhotonPlayer player in PhotonNetwork.PlayerListOthers)
                Check(player);
        }

        public void OnLeftRoom()
        {
            _reported.Clear();
        }

        public void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(VersionKey))
                Check(targetPlayer);
        }

        public void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
            Check(newPlayer);
        }

        public void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            _reported.Remove(otherPlayer.ActorNumber);
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
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

        private void Check(PhotonPlayer player)
        {
            if (player.IsLocal || !(player.CustomProperties[VersionKey] is string version))
                return;

            if (_reported.TryGetValue(player.ActorNumber, out string known) && known == version)
                return;

            _reported[player.ActorNumber] = version;

            if (version == _version)
                _log.LogInfo($"{nameof(ModVersionCheck)}: {player.NickName} has Repkarbinhake {version}");
            else
                _log.LogWarning($"{nameof(ModVersionCheck)}: {player.NickName} has Repkarbinhake {version}, you have {_version} — ropes may behave differently");
        }
    }
}
