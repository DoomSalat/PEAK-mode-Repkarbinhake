using System.Collections;
using System.Globalization;
using ExitGames.Client.Photon;
using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Repkarbinhake.Plugin.Network
{
    public sealed class PhotonLinkTransport : ILinkTransport
    {
        public const string MaxLengthKey = "rkb.cfg.maxLength";

        private const string LocalOnlyMarker = "-";

        private readonly LinkPayloadCodec _codec = new LinkPayloadCodec();

        public bool EchoesOwnUpdates => PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode && PhotonNetwork.CurrentRoom.BroadcastPropertiesChangeToAll;

        public void Publish(LinkUpdate update)
        {
            if (IsLocalOnly(update.Key))
                return;

            Send(update.Key, _codec.EncodeLink(update), null);
        }

        public void PublishEnd(EndUpdate update)
        {
            if (IsLocalOnly(update.Key))
                return;

            Send(update.Key, _codec.EncodeEnd(update.State), null);
        }

        public void DeleteIfUnchanged(LinkUpdate expected)
        {
            Send(expected.Key, null, new Hashtable { { expected.Key, _codec.EncodeLink(expected) } });

            if (!LinkKeys.TryParse(expected.Key, out int firstActor, out int secondActor))
                return;

            DeleteEndIfUnchanged(LinkKeys.BuildEnd(firstActor, secondActor, firstActor));
            DeleteEndIfUnchanged(LinkKeys.BuildEnd(firstActor, secondActor, secondActor));
        }

        public void PublishMaxLength(float maxLength)
        {
            Send(MaxLengthKey, maxLength.ToString("R", CultureInfo.InvariantCulture), null);
        }

        public bool TryReadMaxLength(Hashtable properties, out float maxLength)
        {
            maxLength = 0f;
            return properties[MaxLengthKey] is string text && float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out maxLength);
        }

        public void Dispatch(Hashtable properties, LinkRegistry registry, UntiedKeySweeper sweeper, float now)
        {
            foreach (DictionaryEntry entry in properties)
            {
                if (!(entry.Key is string key))
                    continue;

                string payload = entry.Value as string;

                if (key.StartsWith(LinkKeys.Prefix))
                    DispatchLink(key, payload, registry, sweeper, now);
                else if (key.StartsWith(LinkKeys.EndPrefix) && _codec.TryDecodeEnd(key, payload, out EndUpdate end))
                    registry.ReceiveEnd(end);
            }
        }

        private void DispatchLink(string key, string payload, LinkRegistry registry, UntiedKeySweeper sweeper, float now)
        {
            if (payload == null)
            {
                sweeper.Forget(key);
                return;
            }

            if (!_codec.TryDecodeLink(key, payload, out LinkUpdate update))
                return;

            sweeper.Observe(update, now);
            registry.Receive(update);
        }

        private void DeleteEndIfUnchanged(string endKey)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties[endKey] is string current)
                Send(endKey, null, new Hashtable { { endKey, current } });
        }

        private bool IsLocalOnly(string key)
        {
            return key.Contains(LocalOnlyMarker);
        }

        private void Send(string key, string value, Hashtable expected)
        {
            if (!PhotonNetwork.InRoom)
                return;

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { key, value } }, expected);
        }
    }
}
