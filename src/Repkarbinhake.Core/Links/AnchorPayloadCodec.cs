using System.Globalization;
using System.Numerics;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class AnchorPayloadCodec
    {
        public const string KeyPrefix = "rkb.anchor.";

        private const char Separator = '|';
        private const int Fields = 9;
        private const string Number = "R";

        public string KeyOf(int owner)
        {
            return KeyPrefix + owner.ToString(CultureInfo.InvariantCulture);
        }

        public bool TryParseKey(string key, out int owner)
        {
            owner = 0;
            return key.StartsWith(KeyPrefix) && int.TryParse(key.Substring(KeyPrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out owner) && owner > 0;
        }

        public string Encode(AnchorPlacement placement)
        {
            float[] values =
            {
                placement.Position.X, placement.Position.Y, placement.Position.Z,
                placement.Forward.X, placement.Forward.Y, placement.Forward.Z,
                placement.Up.X, placement.Up.Y, placement.Up.Z,
            };

            string[] parts = new string[Fields];

            for (int i = 0; i < Fields; i++)
                parts[i] = values[i].ToString(Number, CultureInfo.InvariantCulture);

            return string.Join(Separator.ToString(), parts);
        }

        public bool TryDecode(string payload, out AnchorPlacement placement)
        {
            placement = default;

            if (string.IsNullOrEmpty(payload))
                return false;

            string[] parts = payload.Split(Separator);

            if (parts.Length != Fields)
                return false;

            float[] values = new float[Fields];

            for (int i = 0; i < Fields; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                    return false;
            }

            placement = new AnchorPlacement(new Vector3(values[0], values[1], values[2]), new Vector3(values[3], values[4], values[5]), new Vector3(values[6], values[7], values[8]));
            return true;
        }
    }
}
