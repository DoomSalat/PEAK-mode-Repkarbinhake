using System;
using System.Numerics;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Core.Tests.Fakes
{
    public sealed class PyramidObstacles : IRopeObstacles
    {
        private const float Step = 0.005f;
        private const float GradientStep = 0.001f;

        private readonly float _height;
        private readonly float _slope;

        public PyramidObstacles(float height, float slope)
        {
            _height = height;
            _slope = slope;
        }

        public bool TryHit(Vector3 from, Vector3 to, float radius, out Vector3 point, out Vector3 normal)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;

            if (Distance(from) < radius)
                return false;

            float length = Vector3.Distance(from, to);
            int steps = Math.Max(1, (int)(length / Step));

            for (int i = 1; i <= steps; i++)
            {
                Vector3 sample = Vector3.Lerp(from, to, (float)i / steps);

                if (Distance(sample) >= radius)
                    continue;

                normal = Gradient(sample);
                point = sample - normal * Distance(sample);
                return true;
            }

            return false;
        }

        public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
        {
            if (TryHit(from, to, radius, out Vector3 point, out Vector3 normal))
            {
                Vector3 resting = point + normal * radius;
                Vector3 rest = to - resting;
                to = resting + rest - normal * Vector3.Dot(rest, normal);
            }

            float distance = Distance(to);
            return distance < radius ? to + Gradient(to) * (radius - distance) : to;
        }

        private float Distance(Vector3 point)
        {
            float spread = MathF.Max(MathF.Abs(point.X), MathF.Abs(point.Z));
            return (point.Y - (_height - _slope * spread)) / MathF.Sqrt(1f + _slope * _slope);
        }

        private Vector3 Gradient(Vector3 point)
        {
            Vector3 gradient = new Vector3(
                Distance(point + Vector3.UnitX * GradientStep) - Distance(point - Vector3.UnitX * GradientStep),
                Distance(point + Vector3.UnitY * GradientStep) - Distance(point - Vector3.UnitY * GradientStep),
                Distance(point + Vector3.UnitZ * GradientStep) - Distance(point - Vector3.UnitZ * GradientStep));
            return Vector3.Normalize(gradient);
        }
    }
}
