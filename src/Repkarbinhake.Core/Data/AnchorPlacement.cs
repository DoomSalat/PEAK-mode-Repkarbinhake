using System.Numerics;

namespace Repkarbinhake.Core.Data
{
    public readonly struct AnchorPlacement
    {
        public AnchorPlacement(Vector3 position, Vector3 forward, Vector3 up)
        {
            Position = position;
            Forward = forward;
            Up = up;
        }

        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public Vector3 Up { get; }
    }
}
