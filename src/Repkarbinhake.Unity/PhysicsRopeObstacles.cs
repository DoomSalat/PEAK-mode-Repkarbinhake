using Repkarbinhake.Core.Visual;
using UnityEngine;
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Unity
{
    public sealed class PhysicsRopeObstacles : IRopeObstacles
    {
        private const float MinimumTravel = 0.0001f;
        private const int MaxOverlaps = 8;
        private const string ProbeName = "Repkarbinhake Rope Probe";
        private const string ProbeLayer = "Ignore Raycast";

        private static readonly Vector3 ProbeParkingPosition = new Vector3(0f, -10000f, 0f); // Static: constant far-away spot for the collision probe

        private readonly int _layerMask;
        private readonly SphereCollider _probe;
        private readonly Collider[] _overlaps = new Collider[MaxOverlaps];

        public PhysicsRopeObstacles(int layerMask, Transform parent)
        {
            _layerMask = layerMask;
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
            Vector3 start = from.ToUnity();
            Vector3 travel = to.ToUnity() - start;
            float distance = travel.magnitude;

            if (distance < MinimumTravel || !Physics.SphereCast(start, radius, travel / distance, out RaycastHit hit, distance, _layerMask, QueryTriggerInteraction.Ignore))
                return false;

            point = hit.point.ToNumerics();
            normal = hit.normal.ToNumerics();
            return true;
        }

        private Vector3 Sweep(Vector3 start, Vector3 end, float radius)
        {
            Vector3 travel = end - start;
            float distance = travel.magnitude;

            if (distance < MinimumTravel)
                return end;

            if (!Physics.SphereCast(start, radius, travel / distance, out RaycastHit hit, distance, _layerMask, QueryTriggerInteraction.Ignore))
                return end;

            Vector3 resting = hit.point + hit.normal * radius;
            return resting + Vector3.ProjectOnPlane(end - resting, hit.normal);
        }

        private Vector3 Depenetrate(Vector3 point, float radius)
        {
            _probe.radius = radius;
            int count = Physics.OverlapSphereNonAlloc(point, radius, _overlaps, _layerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Transform other = _overlaps[i].transform;

                if (Physics.ComputePenetration(_probe, point, Quaternion.identity, _overlaps[i], other.position, other.rotation, out Vector3 direction, out float depth))
                    point += direction * depth;
            }

            return point;
        }
    }
}
