using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class TestDummy
    {
        public const int Actor = -1000000;

        private const string ObjectName = "Repkarbinhake Test Dummy";
        private const string BodyShader = "Universal Render Pipeline/Lit";
        private const float HalfHeight = 0.8f;
        private const float Radius = 0.3f;
        private const float Mass = 60f;
        private const float AngularDamping = 2f;
        private const float RopePointHeight = 0.45f;
        private const float GroundProbe = 0.15f;

        private static readonly Color BodyColor = new Color(0.9f, 0.55f, 0.15f); // Static: constant dummy color

        private Rigidbody _body;
        private Collider _collider;

        public bool IsPlaced => _body != null;
        public Vector3 Center => _body.position;
        public Vector3 RopePoint => _body.position + _body.transform.up * RopePointHeight;
        public Vector3 Velocity => _body.linearVelocity;
        public bool IsGrounded { get; private set; }

        public void Refresh()
        {
            IsGrounded = IsPlaced && Physics.Raycast(_body.position, Vector3.down, HalfHeight + GroundProbe, HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap));
        }

        public void Place(Vector3 groundPoint)
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = ObjectName;
            dummy.transform.position = groundPoint + Vector3.up * HalfHeight;
            dummy.transform.localScale = new Vector3(Radius * 2f, HalfHeight, Radius * 2f);
            _collider = dummy.GetComponent<Collider>();
            dummy.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find(BodyShader)) { color = BodyColor };
            _body = dummy.AddComponent<Rigidbody>();
            _body.mass = Mass;
            _body.angularDamping = AngularDamping;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public bool IsHitBy(Ray ray, float maxDistance)
        {
            return _collider.Raycast(ray, out _, maxDistance);
        }

        public void Push(Vector3 acceleration)
        {
            _body.AddForce(acceleration, ForceMode.Acceleration);
        }

        public void Remove()
        {
            Object.Destroy(_body.gameObject);
            _body = null;
        }
    }
}
