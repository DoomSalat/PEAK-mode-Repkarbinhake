using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class RopePathAuthority
    {
        public bool IsShared(RopeLink link)
        {
            return link.FirstActor > 0 && link.SecondActor > 0;
        }

        public int OwnerOf(RopeLink link)
        {
            if (link.First.IsHeld != link.Second.IsHeld)
                return link.First.IsHeld ? link.FirstActor : link.SecondActor;

            return link.FirstActor;
        }

        public bool Owns(RopeLink link, int localActor)
        {
            return !IsShared(link) || OwnerOf(link) == localActor;
        }
    }
}
