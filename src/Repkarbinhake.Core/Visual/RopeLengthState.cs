namespace Repkarbinhake.Core.Visual
{
    public readonly struct RopeLengthState
    {
        public RopeLengthState(float total, float firstReel, float secondReel)
        {
            Total = total;
            FirstReel = firstReel;
            SecondReel = secondReel;
        }

        public float Total { get; }
        public float FirstReel { get; }
        public float SecondReel { get; }
    }
}
