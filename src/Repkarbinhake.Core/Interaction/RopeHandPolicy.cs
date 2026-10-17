using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Interaction
{
    public sealed class RopeHandPolicy
    {
        public bool CanHold(Footing footing)
        {
            return footing == Footing.Grounded || footing == Footing.Airborne;
        }

        public bool CanBrace(Footing footing)
        {
            return footing == Footing.Grounded;
        }

        public bool CanReelIn(Footing footing)
        {
            return footing == Footing.Grounded;
        }

        public bool CanReelOut(Footing footing)
        {
            return CanHold(footing);
        }
    }
}
