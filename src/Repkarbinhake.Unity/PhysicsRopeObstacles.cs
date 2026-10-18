using Repkarbinhake.Core.Visual;
using UnityEngine;
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Unity
{
    public sealed class PhysicsRopeObstacles : IRopeObstacles
    {
        private const float MinimumTravel = 0.0001f;
        private const int MaxOverlaps = 16;
        private const float ShellProbe = 0.75f;
        private const float ShellReachScale = 2f;
        private const int MaxHits = 16;
        private const string ProbeName = "Repkarbinhake Rope Probe";
        private const string ProbeLayer = "Ignore Raycast";

        private static readonly Vector3 ProbeParkingPosition = new Vector3(0f, -10000f, 0f); // Static: constant far-away spot for the collision probe

        private readonly int _layerMask;
        private readonly IColliderFilter _filter;
        private readonly SphereCollider _probe;
        private readonly Collider[] _overlaps = new Collider[MaxOverlaps];
        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];

        public PhysicsRopeObstacles(int layerMask, Transform parent, IColliderFilter filter)
        {
            _layerMask = layerMask;
            _filter = filter;
            GameObject probeObject = new GameObject(ProbeName) { layer = LayerMask.NameToLayer(ProbeLayer) };
            probeObject.transform.SetParent(parent, false);
            probeObject.transform.position = ProbeParkingPosition;
            _probe = probeObject.AddComponent<SphereCollider>();
            _probe.isTrigger = true;
        }

        public NumericsVector3 Resolve(NumericsVector3 from, NumericsVector3 to, float radius)
        {
            Vector3 swept = Sweep(from.ToUnity(), to.ToUnity(), radius);
            return Depenetrate(swept, radius).ToNumerics();
        }

        public bool TryHit(NumericsVector3 from, NumericsVector3 to, float radius, out NumericsVector3 point, out NumericsVector3 normal)
        {
            point = NumericsVector3.Zero;
            normal = NumericsVector3.Zero;

            if (!TryCast(from.ToUnity(), to.ToUnity(), radius, out RaycastHit hit))
                return false;

            point = hit.point.ToNumerics();
            normal = hit.normal.ToNumerics();
            return true;
        }

        private bool TryCast(Vector3 start, Vector3 end, float radius, out RaycastHit nearest)
        {
            nearest = default;
            Vector3 travel = end - start;
            float distance = travel.magnitude;

            if (distance < MinimumTravel)
                return false;

            int count = Physics.SphereCastNonAlloc(start, radius, travel / distance, _hits, distance, _layerMask, QueryTriggerInteraction.Ignore);
            bool found = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                bool startedInside = hit.distance <= 0f && hit.point == Vector3.zero;

                if (startedInside || (found && hit.distance >= nearest.distance) || _filter.Ignores(hit.collider))
                    continue;

                nearest = hit;
                found = true;
            }

            return found;
        }

        private Vector3 Sweep(Vector3 start, Vector3 end, float radius)
        {
            if (!TryCast(start, end, radius, out RaycastHit hit))
                return end;

            Vector3 resting = hit.point + hit.normal * radius;
            return resting + Vector3.ProjectOnPlane(end - resting, hit.normal);
        }

        private Vector3 Depenetrate(Vector3 point, float radius)
        {
            _probe.radius = radius;
            int count = Physics.OverlapSphereNonAlloc(point, Mathf.Max(radius, ShellProbe), _overlaps, _layerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider other = _overlaps[i];

                if (_filter.Ignores(other))
                    continue;

                ShellShape shell = _filter.ShellOf(other);

                if (shell != ShellShape.None)
                    point = KeepOutside(point, radius, other, shell);
                else if (Physics.ComputePenetration(_probe, point, Quaternion.identity, other, other.transform.position, other.transform.rotation, out Vector3 direction, out float depth))
                    point += direction * depth;
            }

            return point;
        }

        private Vector3 KeepOutside(Vector3 point, float radius, Collider shell, ShellShape shape)
        {
            Bounds bounds = shell.bounds;

            if (shape == ShellShape.Top && point.y < bounds.min.y - radius)
                return point;

            Vector3 outward = shape == ShellShape.Top ? Vector3.up : shape == ShellShape.Slab ? SlabOutward(point, bounds) : RoundOutward(point, bounds);
            float reach = bounds.extents.magnitude * ShellReachScale + radius;
            bool hitBackfaces = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;
            bool covered = shell.Raycast(new Ray(point + outward * reach, -outward), out RaycastHit hit, reach + radius);
            Physics.queriesHitBackfaces = hitBackfaces;
            return covered ? hit.point + outward * radius : point;
        }

        private Vector3 RoundOutward(Vector3 point, Bounds bounds)
        {
            Vector3 outward = point - bounds.center;
            return outward.sqrMagnitude < MinimumTravel ? Vector3.up : outward.normalized;
        }

        private Vector3 SlabOutward(Vector3 point, Bounds bounds)
        {
            Vector3 extents = bounds.extents;
            Vector3 axis = extents.y <= extents.x && extents.y <= extents.z ? Vector3.up : extents.x <= extents.z ? Vector3.right : Vector3.forward;
            return Vector3.Dot(point - bounds.center, axis) < 0f ? -axis : axis;
        }
    }
}
