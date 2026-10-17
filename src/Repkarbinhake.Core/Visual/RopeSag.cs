using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeSag
    {
        public const float MaxFreedom = 0.45f;
        public const float TautFreedom = 0.03f;

        private const float MinSideways = 0.1f;
        private const float PeakFactor = 4f;
        private const float FreedomPerDepth = 0.8f;
        private const int DepthSearchSteps = 20;

        public void Build(RopeWrapPath path, Vector3 down, int bones, float radius, IRopeObstacles obstacles, List<Vector3> points, List<float> freedoms, List<float> spacings)
        {
            points.Clear();
            freedoms.Clear();
            spacings.Clear();
            IReadOnlyList<Vector3> corners = path.Points;

            if (corners.Count < 2)
                return;

            float pathLength = path.PathLength();
            points.Add(corners[0]);
            freedoms.Add(0f);

            for (int k = 0; k < corners.Count - 1; k++)
            {
                float chord = path.Chord(k);
                float share = pathLength < float.Epsilon ? 1f : chord / pathLength;
                int count = Math.Max(1, (int)MathF.Round(bones * share));
                float slack = MathF.Max(path.Materials[k] - chord, 0f);
                AddSegment(corners[k], corners[k + 1], slack, down, count, radius, obstacles, points, freedoms);

                for (int j = 0; j < count; j++)
                    spacings.Add(path.Materials[k] / count);
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

        private void AddSegment(Vector3 from, Vector3 to, float slack, Vector3 down, int count, float radius, IRopeObstacles obstacles, List<Vector3> points, List<float> freedoms)
        {
            Vector3 along = to - from;
            float length = along.Length();
            Vector3 direction = length < float.Epsilon ? Vector3.Zero : along / length;
            Vector3 sideways = down - direction * Vector3.Dot(down, direction);
            float depth = 0f;

            if (sideways.Length() >= MinSideways && slack > 0f)
            {
                sideways = Vector3.Normalize(sideways);
                depth = SagDepth(length, length + slack);
            }

            float freedom = MathF.Min(MaxFreedom, TautFreedom + MathF.Max(depth, slack) * FreedomPerDepth);

            for (int j = 1; j < count; j++)
            {
                float t = (float)j / count;
                Vector3 straight = Vector3.Lerp(from, to, t);
                Vector3 point = straight + sideways * (depth * PeakFactor * t * (1f - t));

                if (obstacles != null && obstacles.TryHit(straight, point, radius, out Vector3 floor, out Vector3 normal))
                    point = floor + normal * radius;

                points.Add(point);
                freedoms.Add(freedom * MathF.Sin(MathF.PI * t));
            }

            points.Add(to);
            freedoms.Add(0f);
        }
    }
}
