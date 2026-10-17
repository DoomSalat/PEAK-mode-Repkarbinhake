namespace Repkarbinhake.Plugin.Handling
{
    public readonly struct RopeHoldPose
    {
        public RopeHoldPose(int actor, float spoolAngle)
        {
            Actor = actor;
            SpoolAngle = spoolAngle;
        }

        public int Actor { get; }
        public float SpoolAngle { get; }
    }
}
