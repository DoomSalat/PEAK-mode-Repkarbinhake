using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public abstract class PhotonRoomCallbacks : IInRoomCallbacks, IMatchmakingCallbacks
    {
        public virtual void Enable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (PhotonNetwork.InRoom)
                OnJoinedRoom();
        }

        public virtual void Disable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public virtual void OnJoinedRoom()
        {
        }

        public virtual void OnLeftRoom()
        {
        }

        public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public virtual void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
        {
        }

        public virtual void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
        }

        public virtual void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
        }

        public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
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
    }
}
