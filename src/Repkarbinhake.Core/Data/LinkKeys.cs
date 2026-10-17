namespace Repkarbinhake.Core.Data
{
    public static class LinkKeys // Static: one key format shared by every client, no state
    {
        public const string Prefix = "rkb.link.";
        public const string EndPrefix = "rkb.end.";

        private const char PairSeparator = '_';
        private const char ActorSeparator = '.';

        public static string Build(int firstActor, int secondActor)
        {
            return Prefix + Pair(firstActor, secondActor);
        }

        public static string BuildEnd(int firstActor, int secondActor, int endActor)
        {
            return EndPrefix + Pair(firstActor, secondActor) + ActorSeparator + endActor;
        }

        public static long PairId(int firstActor, int secondActor)
        {
            int low = firstActor < secondActor ? firstActor : secondActor;
            int high = firstActor < secondActor ? secondActor : firstActor;
            return ((long)low << 32) | (uint)high;
        }

        public static bool TryParse(string key, out int firstActor, out int secondActor)
        {
            firstActor = 0;
            secondActor = 0;

            if (key == null || !key.StartsWith(Prefix))
                return false;

            return TryParsePair(key.Substring(Prefix.Length), out firstActor, out secondActor);
        }

        public static bool TryParseEnd(string key, out int firstActor, out int secondActor, out int endActor)
        {
            firstActor = 0;
            secondActor = 0;
            endActor = 0;

            if (key == null || !key.StartsWith(EndPrefix))
                return false;

            string[] parts = key.Substring(EndPrefix.Length).Split(ActorSeparator);

            if (parts.Length != 2 || !TryParsePair(parts[0], out firstActor, out secondActor))
                return false;

            return int.TryParse(parts[1], out endActor) && (endActor == firstActor || endActor == secondActor);
        }

        private static string Pair(int firstActor, int secondActor)
        {
            int low = firstActor < secondActor ? firstActor : secondActor;
            int high = firstActor < secondActor ? secondActor : firstActor;
            return low.ToString() + PairSeparator + high;
        }

        private static bool TryParsePair(string pair, out int firstActor, out int secondActor)
        {
            firstActor = 0;
            secondActor = 0;
            string[] parts = pair.Split(PairSeparator);

            if (parts.Length != 2)
                return false;

            return int.TryParse(parts[0], out firstActor) && int.TryParse(parts[1], out secondActor) && firstActor != secondActor;
        }
    }
}
