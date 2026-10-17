using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeSag
    {
        private const float ParabolaLength = 3f / 8f;
        private const float MinSideways = 0.1f;
        private const float PeakFactor = 4f;
        private const float SwingStiffness = 20f;
        private const float SwingDamping = 2.5f;
        private const float MaxSwing = 0.6f;

        private static readonly Vector3 Unset = new Vector3(float.NaN); // Static: marker for a segment without a previous rest position

        private readonly List<Vector3> _restPeaks = new List<Vector3>();
        private readonly List<Vector3> _swings = new List<Vector3>();
        private readonly List<Vector3> _swingSpeeds = new List<Vector3>();
        private readonly List<bool> _grounded = new List<bool>();
        private int _pathVersion = -1;

        public void Build(RopeWrapPath path, Vector3 down, int bones, float radius, float deltaTime, IRopeObstacles obstacles, List<Vector3> output)
        {
            output.Clear();
            IReadOnlyList<Vector3> points = path.Points;

            if (points.Count < 2)
                return;

            ResetSwingsIfChanged(path);
            float pathLength = path.PathLength();
            output.Add(points[0]);

            for (int k = 0; k < points.Count - 1; k++)
            {
                float chord = path.Chord(k);
                float share = pathLength < float.Epsilon ? 1f : chord / pathLength;
                int count = Math.Max(1, (int)MathF.Round(bones * share));
                float slack = MathF.Max(path.Materials[k] - chord, 0f);
                AddSegment(k, points[k], points[k + 1], slack, down, count, radius, deltaTime, obstacles, output);
            }
        }

        private void AddSegment(int segment, Vector3 from, Vector3 to, float slack, Vector3 down, int count, float radius, float deltaTime, IRopeObstacles obstacles, List<Vector3> output)
        {
            Vector3 along = to - from;
            float length = along.Length();
            Vector3 direction = length < float.Epsilon ? Vector3.Zero : along / length;
            Vector3 sideways = down - direction * Vector3.Dot(down, direction);
            float depth = 0f;

            if (sideways.Length() >= MinSideways && slack > 0f)
            {
                sideways = Vector3.Normalize(sideways);
                depth = MathF.Sqrt(ParabolaLength * length * slack);
            }

            Vector3 restPeak = Vector3.Lerp(from, to, 0.5f) + sideways * depth;
            Vector3 swing = StepSwing(segment, restPeak, direction, _grounded[segment] ? 0f : MathF.Min(depth, MaxSwing), deltaTime);
            bool grounded = false;

            for (int j = 1; j < count; j++)
            {
                float t = (float)j / count;
                float bulge = PeakFactor * t * (1f - t);
                Vector3 straight = Vector3.Lerp(from, to, t);
                Vector3 sagged = straight + (sideways * depth + swing) * bulge;
                if (obstacles != null && obstacles.TryHit(straight, sagged, radius, out Vector3 point, out Vector3 normal))
                {
                    sagged = point + normal * radius;
                    grounded = true;
                }

                output.Add(sagged);
            }

            _grounded[segment] = grounded;
            output.Add(to);
        }

        private Vector3 StepSwing(int segment, Vector3 restPeak, Vector3 direction, float limit, float deltaTime)
        {
            if (float.IsNaN(_restPeaks[segment].X))
                _restPeaks[segment] = restPeak;

            Vector3 swing = _swings[segment] - (restPeak - _restPeaks[segment]);
            Vector3 speed = _swingSpeeds[segment];
            speed += (-SwingStiffness * swing - SwingDamping * speed) * deltaTime;
            swing += speed * deltaTime;
            swing -= direction * Vector3.Dot(swing, direction);

            if (swing.Length() > limit)
            {
                swing = limit < float.Epsilon ? Vector3.Zero : Vector3.Normalize(swing) * limit;
                speed = Vector3.Zero;
            }

            _restPeaks[segment] = restPeak;
            _swings[segment] = swing;
            _swingSpeeds[segment] = speed;
            return swing;
        }

        private void ResetSwingsIfChanged(RopeWrapPath path)
        {
            int segments = path.Points.Count - 1;

            if (path.Version == _pathVersion && _swings.Count == segments)
                return;

            _pathVersion = path.Version;
            _restPeaks.Clear();
            _swings.Clear();
            _swingSpeeds.Clear();
            _grounded.Clear();

            for (int k = 0; k < segments; k++)
            {
                _restPeaks.Add(Unset);
                _swings.Add(Vector3.Zero);
                _swingSpeeds.Add(Vector3.Zero);
                _grounded.Add(false);
            }
        }
    }
}
