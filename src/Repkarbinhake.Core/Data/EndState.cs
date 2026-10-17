namespace Repkarbinhake.Core.Data
{
    public readonly struct EndState
    {
        public EndState(long tieId, bool isHeld, float reel)
        {
            TieId = tieId;
            IsHeld = isHeld;
            Reel = reel;
        }

        public long TieId { get; }
        public bool IsHeld { get; }
        public float Reel { get; }
    }
}
