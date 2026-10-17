using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class SpoolProp : MonoBehaviour
    {
        [SerializeField] private Transform _wheel;
        [SerializeField] private Transform _ropeStart;
        [SerializeField] private Transform _ropeBase;
        [SerializeField] private Vector3 _gripCenter;
        [SerializeField] private Quaternion _wheelRest;

        public Vector3 RopeStart => _ropeStart.position;
        public Vector3 RopeBase => _ropeBase.position;

        public void Initialize(Transform wheel, Transform ropeStart, Transform ropeBase, Vector3 gripCenter)
        {
            _wheel = wheel;
            _ropeStart = ropeStart;
            _ropeBase = ropeBase;
            _gripCenter = gripCenter;
            _wheelRest = wheel.localRotation;
        }

        public void Place(Vector3 grip, Quaternion rotation, float wheelAngle)
        {
            transform.rotation = rotation;
            transform.position += grip - transform.TransformPoint(_gripCenter);
            _wheel.localRotation = _wheelRest * Quaternion.Euler(0f, 0f, wheelAngle);
        }
    }
}
