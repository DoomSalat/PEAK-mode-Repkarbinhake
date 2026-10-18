namespace Repkarbinhake.Plugin.Visual
{
    public readonly struct HookIndicatorState
    {
        public HookIndicatorState(bool placed, bool locked, bool atMax, bool retracting, float meters)
        {
            Placed = placed;
            Locked = locked;
            AtMax = atMax;
            Retracting = retracting;
            Meters = meters;
        }

        public bool Placed { get; }
        public bool Locked { get; }
        public bool AtMax { get; }
        public bool Retracting { get; }
        public float Meters { get; }
    }
}
