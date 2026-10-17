namespace Repkarbinhake.Core.Data
{
    public sealed class RopeEnd
    {
        public RopeEnd(int actor)
        {
            Actor = actor;
        }

        public int Actor { get; }
        public bool IsHeld { get; set; } // Set: changed by the owner locally and by network updates of the owner
        public float Reel { get; set; } // Set: changed by the owner locally and by network updates of the owner
        public float HeldReel => IsHeld ? Reel : 0f;

        public void Reset()
        {
            IsHeld = false;
            Reel = 0f;
        }
    }
}
