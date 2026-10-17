using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Plugin.Visual;
using PhotonPlayer = Photon.Realtime.Player;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class RopePathSync : PhotonRoomCallbacks, IOnEventCallback
    {
        private const byte EventCode = 173;
        private const float SendInterval = 0.1f;
        private const float SameTolerance = 0.01f;
        private const int KeyIndex = 0;
        private const int TieIndex = 1;
        private const int DataIndex = 2;
        private const int PayloadSize = 3;

        private static readonly RaiseEventOptions ToOthers = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; // Static: constant send options

        private readonly LinkRegistry _registry;
        private readonly RopePresenterPool _ropes;
        private readonly RopePathAuthority _authority = new RopePathAuthority();
        private readonly RopePathCodec _codec = new RopePathCodec();
        private readonly Dictionary<string, float[]> _lastSent = new Dictionary<string, float[]>();
        private readonly List<System.Numerics.Vector3> _contacts = new List<System.Numerics.Vector3>();
        private readonly List<float> _materials = new List<float>();
        private float _sendTimer;
        private bool _resendAll;

        public RopePathSync(LinkRegistry registry, RopePresenterPool ropes)
        {
            _registry = registry;
            _ropes = ropes;
        }

        public void Tick(int localActor, float deltaTime)
        {
            foreach (RopeLink link in _registry.Links)
            {
                if (_authority.Owns(link, localActor) && _ropes.TryGetPath(link, out RopeWrapPath path) && path.IsFollowing)
                    path.Lead();
            }

            _sendTimer += deltaTime;

            if (_sendTimer < SendInterval)
                return;

            _sendTimer = 0f;

            foreach (RopeLink link in _registry.Links)
            {
                if (_authority.IsShared(link) && _authority.OwnerOf(link) == localActor)
                    Send(link);
            }

            _resendAll = false;
        }

        public void Forget(RopeLink link)
        {
            _lastSent.Remove(link.Key);
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != EventCode || !(photonEvent.CustomData is object[] payload) || payload.Length != PayloadSize)
                return;

            if (!(payload[KeyIndex] is string key) || !(payload[TieIndex] is long tieId) || !(payload[DataIndex] is float[] data))
                return;

            if (!LinkKeys.TryParse(key, out int first, out int second) || !_registry.TryGet(first, second, out RopeLink link) || link.TieId != tieId)
                return;

            if (_authority.Owns(link, _registry.LocalActor) || !_ropes.TryGetPath(link, out RopeWrapPath path) || !_codec.TryDecode(data, _contacts, _materials))
                return;

            path.Follow(_contacts, _materials);
        }

        public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
            _resendAll = true;
        }

        private void Send(RopeLink link)
        {
            if (!_ropes.TryGetPath(link, out RopeWrapPath path) || path.Points.Count < 2)
                return;

            float[] data = _codec.Encode(path.Points, path.Materials);

            if (!_resendAll && _lastSent.TryGetValue(link.Key, out float[] last) && IsSame(last, data))
                return;

            _lastSent[link.Key] = data;
            PhotonNetwork.RaiseEvent(EventCode, new object[] { link.Key, link.TieId, data }, ToOthers, SendOptions.SendReliable);
        }

        private bool IsSame(float[] last, float[] data)
        {
            if (last.Length != data.Length)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                if (System.Math.Abs(last[i] - data[i]) > SameTolerance)
                    return false;
            }

            return true;
        }
    }
}
