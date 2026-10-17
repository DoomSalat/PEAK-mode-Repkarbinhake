using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeRayPicker
    {
        private const float ParallelThreshold = 1e-6f;

        public bool TryPick(Vector3 origin, Vector3 direction, float maxDistance, float tolerance, IReadOnlyList<Vector3> points, out float hitDistance)
        {
            hitDistance = float.MaxValue;
            bool found = false;
            float toleranceSquared = tolerance * tolerance;

            for (int i = 1; i < points.Count; i++)
            {
                ClosestBetweenRayAndSegment(origin, direction, points[i - 1], points[i], out float rayDistance, out float gapSquared);

                if (gapSquared > toleranceSquared || rayDistance < 0f || rayDistance > maxDistance || rayDistance >= hitDistance)
                    continue;

                hitDistance = rayDistance;
                found = true;
            }

            return found;
        }

        private void ClosestBetweenRayAndSegment(Vector3 origin, Vector3 direction, Vector3 start, Vector3 end, out float rayDistance, out float gapSquared)
        {
            Vector3 segment = end - start;
            Vector3 offset = origin - start;
            float segmentLengthSquared = Vector3.Dot(segment, segment);
            float alignment = Vector3.Dot(direction, segment);
            float rayToOffset = Vector3.Dot(direction, offset);
            float segmentToOffset = Vector3.Dot(segment, offset);
            float denominator = segmentLengthSquared - alignment * alignment;
            float segmentFactor = denominator > ParallelThreshold ? (segmentToOffset - alignment * rayToOffset) / denominator : 0f;

            segmentFactor = segmentLengthSquared > ParallelThreshold ? Math.Clamp(segmentFactor, 0f, 1f) : 0f;
            Vector3 onSegment = start + segment * segmentFactor;
            rayDistance = Vector3.Dot(onSegment - origin, direction);
            Vector3 onRay = origin + direction * rayDistance;
            gapSquared = Vector3.DistanceSquared(onRay, onSegment);
        }
    }
}
