using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeSag
    {
        public const float MaxSwing = 0.6f;
        public const float GroundSwing = 0.2f;
        public const float MaxWave = 0.35f;
        public const float WaveLength = 1.2f;

        private const float MinSideways = 0.1f;
        private const float PeakFactor = 4f;
        private const float SwingStiffness = 20f;
        private const float SwingDamping = 2.5f;
        private const float GroundDamping = 8f;
        private const float WaveFollowRate = 3f;
        private const float RollRate = 2f;
        private const int DepthSearchSteps = 20;
        private const float FullTurn = MathF.PI * 2f;

        private static readonly Vector3 Unset = new Vector3(float.NaN); // Static: marker for a segment without a previous rest position

        private readonly List<Vector3> _restPeaks = new List<Vector3>();
        private readonly List<Vector3> _swings = new List<Vector3>();
        private readonly List<Vector3> _swingSpeeds = new List<Vector3>();
        private readonly List<bool> _grounded = new List<bool>();
        private readonly List<float> _waves = new List<float>();
        private readonly List<float> _phases = new List<float>();
        private int _pathVersion = -1;

        public void Build(RopeWrapPath path, Vector3 down, int bones, float radius, float deltaTime, IRopeObstacles obstacles, List<Vector3> output)
        {
            output.Clear();
            IReadOnlyList<Vector3> points = path.Points;

            if (points.Count < 2)
                return;

            ResetIfChanged(path);
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

        public static float SagDepth(float chord, float material) // Static: pure geometry helper, also used by tests
        {
            if (material <= chord || chord < float.Epsilon)
                return 0f;

            float low = 0f;
            float high = material;

            for (int step = 0; step < DepthSearchSteps; step++)
            {
                float middle = (low + high) * 0.5f;

                if (ParabolaLength(chord, middle) < material)
                    low = middle;
                else
                    high = middle;
            }

            return (low + high) * 0.5f;
        }

        public static float ParabolaLength(float chord, float depth) // Static: pure geometry helper, also used by tests
        {
            if (depth < float.Epsilon)
                return chord;

            float ratio = 4f * depth / chord;
            float root = MathF.Sqrt(1f + ratio * ratio);
            return chord * 0.5f * root + chord * chord / (8f * depth) * MathF.Log(ratio + root);
        }

        private void AddSegment(int segment, Vector3 from, Vector3 to, float slack, Vector3 down, int count, float radius, float deltaTime, IRopeObstacles obstacles, List<Vector3> output)
        {
            Vector3 along = to - from;
            float length = along.Length();
            Vector3 direction = length < float.Epsilon ? Vector3.Zero : along / length;
            Vector3 sideways = down - direction * Vector3.Dot(down, direction);
            Vector3 lateral = Vector3.Cross(direction, down);
            float depth = 0f;
            bool canSag = sideways.Length() >= MinSideways && slack > 0f;

            if (canSag)
            {
                sideways = Vector3.Normalize(sideways);
                lateral = Vector3.Normalize(lateral);
                depth = SagDepth(length, length + slack);
            }

            bool wasGrounded = _grounded[segment];
            float swingLimit = MathF.Min(depth, wasGrounded ? GroundSwing : MaxSwing);
            Vector3 restPeak = Vector3.Lerp(from, to, 0.5f) + sideways * depth;
            Vector3 swing = StepSwing(segment, restPeak, direction, swingLimit, wasGrounded ? GroundDamping : SwingDamping, deltaTime);
            float wave = StepWave(segment, canSag && wasGrounded ? WaveAmplitude(length, slack) : 0f, deltaTime);
            int waves = WaveCount(length);
            bool grounded = false;

            for (int j = 1; j < count; j++)
            {
                float t = (float)j / count;
                float bulge = PeakFactor * t * (1f - t);
                Vector3 straight = Vector3.Lerp(from, to, t);
                Vector3 point = straight + (sideways * depth + swing) * bulge;

                if (obstacles != null && obstacles.TryHit(straight, point, radius, out Vector3 floor, out Vector3 normal))
                {
                    point = floor + normal * radius;
                    grounded = true;
                }

                if (wave > 0f)
                    point = Offset(point, lateral * (wave * MathF.Sin(FullTurn * waves * t + _phases[segment]) * MathF.Sin(MathF.PI * t)), radius, obstacles);

                output.Add(point);
            }

            _grounded[segment] = grounded;
            output.Add(to);
        }

        private Vector3 Offset(Vector3 point, Vector3 offset, float radius, IRopeObstacles obstacles)
        {
            Vector3 target = point + offset;

            if (obstacles == null || !obstacles.TryHit(point, target, radius, out Vector3 hit, out Vector3 normal))
                return target;

            return hit + normal * radius;
        }

        private int WaveCount(float length)
        {
            return Math.Max(1, (int)MathF.Round(length / WaveLength));
        }

        private float WaveAmplitude(float length, float slack)
        {
            return MathF.Min(MathF.Sqrt(slack * length) / (MathF.PI * WaveCount(length)), MaxWave);
        }

        private float StepWave(int segment, float target, float deltaTime)
        {
            float share = Math.Clamp(WaveFollowRate * deltaTime, 0f, 1f);
            _waves[segment] += (target - _waves[segment]) * share;
            return _waves[segment];
        }

        private Vector3 StepSwing(int segment, Vector3 restPeak, Vector3 direction, float limit, float damping, float deltaTime)
        {
            if (float.IsNaN(_restPeaks[segment].X))
                _restPeaks[segment] = restPeak;

            Vector3 moved = restPeak - _restPeaks[segment];
            _phases[segment] = (_phases[segment] + Vector3.Dot(moved, direction) * RollRate) % FullTurn;
            Vector3 swing = _swings[segment] - moved;
            Vector3 speed = _swingSpeeds[segment];
            speed += (-SwingStiffness * swing - damping * speed) * deltaTime;
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

        private void ResetIfChanged(RopeWrapPath path)
        {
            int segments = path.Points.Count - 1;

            if (path.Version == _pathVersion && _swings.Count == segments)
                return;

            _pathVersion = path.Version;
            _restPeaks.Clear();
            _swings.Clear();
            _swingSpeeds.Clear();
            _grounded.Clear();
            _waves.Clear();
            _phases.Clear();

            for (int k = 0; k < segments; k++)
            {
                _restPeaks.Add(Unset);
                _swings.Add(Vector3.Zero);
                _swingSpeeds.Add(Vector3.Zero);
                _grounded.Add(false);
                _waves.Add(0f);
                _phases.Add(0f);
            }
        }
    }
}
