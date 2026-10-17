namespace Repkarbinhake.Core.Data
{
    public static class AnchorActors // Static: one numbering rule shared by every client, no state
    {
        public const int MaxOwner = 9999;

        public static int Of(int owner)
        {
            return -owner;
        }

        public static bool IsAnchor(int actor)
        {
            return actor < 0 && actor >= -MaxOwner;
        }

        public static int OwnerOf(int anchor)
        {
            return -anchor;
        }
    }
}
