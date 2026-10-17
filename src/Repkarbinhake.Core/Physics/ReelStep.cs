namespace Repkarbinhake.Core.Physics
{
    public readonly struct ReelStep
    {
        public ReelStep(float reel, float staminaCost)
        {
            Reel = reel;
            StaminaCost = staminaCost;
        }

        public float Reel { get; }
        public float StaminaCost { get; }
    }
}
