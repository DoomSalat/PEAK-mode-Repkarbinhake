using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxCamera : MonoBehaviour
    {
        private const float MinPitch = -30f;
        private const float MaxPitch = 75f;

        [SerializeField] private Camera _camera;
        [SerializeField, Min(0.5f)] private float _distance = 4.5f;
        [SerializeField] private Vector3 _pivotOffset = new Vector3(0f, 0.8f, 0f);
        [SerializeField, Min(0f)] private float _sensitivity = 3f;
        [SerializeField, Min(0f)] private float _followSharpness = 15f;

        private float _yaw;
        private float _pitch = 15f;
        private Vector3 _pivot;

        public Vector3 Forward => _camera.transform.forward;
        public Vector3 FlatForward => Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        public Vector3 FlatRight => Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
        public Vector3 Position => _camera.transform.position;

        public void Look(Vector2 delta)
        {
            _yaw += delta.x * _sensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * _sensitivity, MinPitch, MaxPitch);
        }

        public void Follow(Vector3 target, float deltaTime)
        {
            _pivot = Vector3.Lerp(_pivot, target + _pivotOffset, 1f - Mathf.Exp(-_followSharpness * deltaTime));
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _camera.transform.SetPositionAndRotation(_pivot - rotation * Vector3.forward * _distance, rotation);
        }

        public void Snap(Vector3 target)
        {
            _pivot = target + _pivotOffset;
        }
    }
}
