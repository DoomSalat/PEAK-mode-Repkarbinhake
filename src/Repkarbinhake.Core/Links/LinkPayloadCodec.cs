using System.Globalization;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class LinkPayloadCodec
    {
        private const char Separator = '|';
        private const string Version = "1";
        private const string True = "1";
        private const string False = "0";
        private const int LinkParts = 5;
        private const int EndParts = 4;

        public string EncodeLink(LinkUpdate update)
        {
            return string.Join(Separator.ToString(),
                Version,
                update.State.IsTied ? True : False,
                update.State.TieId.ToString(CultureInfo.InvariantCulture),
                update.WriterActor.ToString(CultureInfo.InvariantCulture),
                update.Sequence.ToString(CultureInfo.InvariantCulture));
        }

        public bool TryDecodeLink(string key, string payload, out LinkUpdate update)
        {
            update = default;
            string[] parts = payload?.Split(Separator);

            if (parts == null || parts.Length != LinkParts || parts[0] != Version)
                return false;

            if (!long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long tieId)
                || !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int writer)
                || !int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sequence))
                return false;

            update = new LinkUpdate(key, new LinkState(parts[1] == True, tieId), writer, sequence);
            return true;
        }

        public string EncodeEnd(EndState state)
        {
            return string.Join(Separator.ToString(),
                Version,
                state.TieId.ToString(CultureInfo.InvariantCulture),
                state.IsHeld ? True : False,
                state.Reel.ToString("R", CultureInfo.InvariantCulture));
        }

        public bool TryDecodeEnd(string key, string payload, out EndUpdate update)
        {
            update = default;
            string[] parts = payload?.Split(Separator);

            if (parts == null || parts.Length != EndParts || parts[0] != Version)
                return false;

            if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long tieId)
                || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float reel))
                return false;

            update = new EndUpdate(key, new EndState(tieId, parts[2] == True, reel));
            return true;
        }
    }
}
