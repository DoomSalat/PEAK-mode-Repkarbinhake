using System;
using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Core.Tests.Fakes
{
    public sealed class BoxObstacles : IRopeObstacles
    {
        private readonly List<(Vector3 Min, Vector3 Max)> _boxes = new List<(Vector3 Min, Vector3 Max)>();

        public BoxObstacles Add(Vector3 min, Vector3 max)
        {
            _boxes.Add((min, max));
            return this;
        }

        public bool TryHit(Vector3 from, Vector3 to, float radius, out Vector3 point, out Vector3 normal)
        {
            point = Vector3.Zero;
            normal = Vector3.Zero;
            float best = float.MaxValue;

            foreach ((Vector3 Min, Vector3 Max) box in _boxes)
            {
                Vector3 min = box.Min - new Vector3(radius);
                Vector3 max = box.Max + new Vector3(radius);

                if (IsInside(from, min, max) || !TryEnter(from, to, min, max, out float t, out Vector3 faceNormal) || t >= best)
                    continue;

                best = t;
                normal = faceNormal;
                point = from + (to - from) * t - faceNormal * radius;
            }

            return best < float.MaxValue;
        }

        public Vector3 Resolve(Vector3 from, Vector3 to, float radius)
        {
            if (!TryHit(from, to, radius, out Vector3 point, out Vector3 normal))
                return PushOut(to, radius);

            Vector3 resting = point + normal * radius;
            Vector3 rest = to - resting;
            return PushOut(resting + rest - normal * Vector3.Dot(rest, normal), radius);
        }

        public bool IsBlocked(Vector3 from, Vector3 to, float radius)
        {
            return TryHit(from, to, radius, out _, out _);
        }

        private Vector3 PushOut(Vector3 point, float radius)
        {
            foreach ((Vector3 Min, Vector3 Max) box in _boxes)
            {
                Vector3 min = box.Min - new Vector3(radius);
                Vector3 max = box.Max + new Vector3(radius);

                if (!IsInside(point, min, max))
                    continue;

                float[] exits = { point.X - min.X, max.X - point.X, point.Y - min.Y, max.Y - point.Y, point.Z - min.Z, max.Z - point.Z };
                int nearest = 0;

                for (int i = 1; i < exits.Length; i++)
                {
                    if (exits[i] < exits[nearest])
                        nearest = i;
                }

                switch (nearest)
                {
                    case 0: point.X = min.X; break;
                    case 1: point.X = max.X; break;
                    case 2: point.Y = min.Y; break;
                    case 3: point.Y = max.Y; break;
                    case 4: point.Z = min.Z; break;
                    default: point.Z = max.Z; break;
                }
            }

            return point;
        }

        private static bool IsInside(Vector3 point, Vector3 min, Vector3 max) // Static: pure geometry helper
        {
            return point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y && point.Z > min.Z && point.Z < max.Z;
        }

        private static bool TryEnter(Vector3 from, Vector3 to, Vector3 min, Vector3 max, out float enter, out Vector3 faceNormal) // Static: pure geometry helper
        {
            enter = 0f;
            float exit = 1f;
            faceNormal = Vector3.Zero;
            Vector3 direction = to - from;
            float[] origin = { from.X, from.Y, from.Z };
            float[] delta = { direction.X, direction.Y, direction.Z };
            float[] low = { min.X, min.Y, min.Z };
            float[] high = { max.X, max.Y, max.Z };

            for (int axis = 0; axis < 3; axis++)
            {
                if (MathF.Abs(delta[axis]) < 1e-6f)
                {
                    if (origin[axis] <= low[axis] || origin[axis] >= high[axis])
                        return false;

                    continue;
                }

                float t1 = (low[axis] - origin[axis]) / delta[axis];
                float t2 = (high[axis] - origin[axis]) / delta[axis];
                float sign = -1f;

                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                    sign = 1f;
                }

                if (t1 > enter)
                {
                    enter = t1;
                    faceNormal = Axis(axis) * sign;
                }

                exit = MathF.Min(exit, t2);

                if (enter > exit)
                    return false;
            }

            return faceNormal != Vector3.Zero;
        }

        private static Vector3 Axis(int axis) // Static: pure geometry helper
        {
            return axis == 0 ? Vector3.UnitX : axis == 1 ? Vector3.UnitY : Vector3.UnitZ;
        }
    }
}
