using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class BodyContacts
    {
        private const string ProbeName = "Repkarbinhake Belt Probe";
        private const string ProbeLayer = "Ignore Raycast";

        private static readonly Vector3 ProbeParkingPosition = new Vector3(0f, -10000f, 0f); // Static: constant far-away spot for the collision probe

        private readonly SphereCollider _probe;

        public BodyContacts(Transform parent)
        {
            GameObject probeObject = new GameObject(ProbeName) { layer = LayerMask.NameToLayer(ProbeLayer) };
            probeObject.transform.SetParent(parent, false);
            probeObject.transform.position = ProbeParkingPosition;
            _probe = probeObject.AddComponent<SphereCollider>();
            _probe.isTrigger = true;
        }

        public Vector3 PushOut(Character character, Vector3 point, float radius)
        {
            _probe.radius = radius;

            foreach (Collider body in character.refs.ragdoll.colliderList)
            {
                if (body == null || !body.enabled)
                    continue;

                Transform bodyTransform = body.transform;

                if (Physics.ComputePenetration(_probe, point, Quaternion.identity, body, bodyTransform.position, bodyTransform.rotation, out Vector3 direction, out float depth))
                    point += direction * depth;
            }

            return point;
        }
    }
}
