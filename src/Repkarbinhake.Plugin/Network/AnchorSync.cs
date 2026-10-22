using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class AnchorSync : PhotonRoomCallbacks
    {
        private readonly AnchorBoard _board;
        private readonly LobbyHookRule _rule;
        private readonly AnchorPayloadCodec _codec = new AnchorPayloadCodec();

        public AnchorSync(AnchorBoard board, LobbyHookRule rule)
        {
            _board = board;
            _rule = rule;
        }

        public override void Enable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void Place(AnchorPlacement placement)
        {
            int owner = PhotonNetwork.LocalPlayer.ActorNumber;
            _board.Set(owner, placement);
            Write(owner, _codec.Encode(placement));
        }

        public void RemoveOwn()
        {
            int owner = PhotonNetwork.LocalPlayer.ActorNumber;
            _board.Remove(owner);
            Write(owner, null);
        }

        public override void OnJoinedRoom()
        {
            _board.Clear();
            Write(PhotonNetwork.LocalPlayer.ActorNumber, null);
            Read(PhotonNetwork.CurrentRoom.CustomProperties);
        }

        public override void OnLeftRoom()
        {
            _board.Clear();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Read(propertiesThatChanged);
        }

        public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            _board.Remove(otherPlayer.ActorNumber);

            if (PhotonNetwork.IsMasterClient)
                Write(otherPlayer.ActorNumber, null);
        }

        private void Read(Hashtable properties)
        {
            int local = PhotonNetwork.LocalPlayer.ActorNumber;

            foreach (DictionaryEntry entry in properties)
            {
                if (!(entry.Key is string key) || !_codec.TryParseKey(key, out int owner) || owner == local)
                    continue;

                if (_rule.Allowed && entry.Value is string payload && _codec.TryDecode(payload, out AnchorPlacement placement) && PhotonNetwork.CurrentRoom.GetPlayer(owner) != null)
                    _board.Set(owner, placement);
                else
                    _board.Remove(owner);
            }
        }

        private void Write(int owner, string payload)
        {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { _codec.KeyOf(owner), payload } });
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!PhotonNetwork.InRoom)
                return;

            if (_board.Has(PhotonNetwork.LocalPlayer.ActorNumber))
                RemoveOwn();

            _board.Clear();
        }
    }
}
