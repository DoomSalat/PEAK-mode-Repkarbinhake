using System.Collections.Generic;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using Photon.Pun;
using Repkarbinhake.Plugin.Patches;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class ModVersionCheck : PhotonRoomCallbacks
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

        public override void OnJoinedRoom()
        {
            _reported.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { VersionKey, _version } });

            foreach (PhotonPlayer player in PhotonNetwork.PlayerListOthers)
                Check(player);
        }

        public override void OnLeftRoom()
        {
            _reported.Clear();
        }

        public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(VersionKey))
                Check(targetPlayer);
        }

        public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
            Check(newPlayer);
        }

        public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            _reported.Remove(otherPlayer.ActorNumber);
        }

        private void Check(PhotonPlayer player)
        {
            if (player.IsLocal || !(player.CustomProperties[VersionKey] is string version))
                return;

            if (_reported.TryGetValue(player.ActorNumber, out string known) && known == version)
                return;

            _reported[player.ActorNumber] = version;

            if (version == _version)
                _log.LogInfo($"{nameof(ModVersionCheck)}: {player.NickName} has {RepkaPlugin.Name} {version}");
            else
                Warn(player, version);
        }

        private void Warn(PhotonPlayer player, string version)
        {
            _log.LogWarning($"{nameof(ModVersionCheck)}: {player.NickName} has {RepkaPlugin.Name} {version}, you have {_version} — ropes may behave differently");
            PlayerConnectionLog feed = RepkaHooks.ConnectionLog;

            if (feed != null)
                feed.AddMessage($"{RepkaPlugin.Name}: {feed.GetColorTag(feed.userColor)}{player.NickName}</color> {version} ≠ {_version}");
        }
    }
}
