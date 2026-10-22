using System.Collections.Generic;
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeEndMask
    {
        private const float Radius = 0.315f;

        public void Apply(List<NumericsVector3> points, NumericsVector3? start, NumericsVector3? end)
        {
            if (start.HasValue)
                Collapse(points, start.Value, 0, 1);

            if (end.HasValue)
                Collapse(points, end.Value, points.Count - 1, -1);
        }

        private void Collapse(List<NumericsVector3> points, NumericsVector3 center, int first, int step)
        {
            int outside = first;

            while (outside >= 0 && outside < points.Count && Inside(points[outside], center))
                outside += step;

            if (outside == first)
                return;

            NumericsVector3 edge = outside >= 0 && outside < points.Count ? Boundary(points[outside - step], points[outside], center) : points[outside - step];

            for (int i = first; i != outside; i += step)
                points[i] = edge;
        }

        private bool Inside(NumericsVector3 point, NumericsVector3 center)
        {
            return NumericsVector3.DistanceSquared(point, center) < Radius * Radius;
        }

        private NumericsVector3 Boundary(NumericsVector3 inside, NumericsVector3 outside, NumericsVector3 center)
        {
            NumericsVector3 travel = outside - inside;
            NumericsVector3 offset = inside - center;
            float quadratic = travel.LengthSquared();
            float linear = 2f * NumericsVector3.Dot(offset, travel);
            float constant = offset.LengthSquared() - Radius * Radius;
            float discriminant = linear * linear - 4f * quadratic * constant;

            if (quadratic <= 0f || discriminant < 0f)
                return outside;

            float share = (-linear + (float)System.Math.Sqrt(discriminant)) / (2f * quadratic);
            return inside + travel * share;
        }
    }
}
