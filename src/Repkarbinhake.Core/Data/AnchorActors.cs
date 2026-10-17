namespace Repkarbinhake.Core.Data
{
    public static class AnchorActors // Static: one numbering rule shared by every client, no state
    {
        public static int Of(int owner)
        {
            return -owner;
        }

        public static bool IsAnchor(int actor)
        {
            return actor < 0;
        }

        public static int OwnerOf(int anchor)
        {
            return -anchor;
        }
    }
}
