using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class SpoolProp
    {
        private readonly GameObject _root;
        private readonly Transform _wheel;
        private readonly Quaternion _wheelRest;
        private readonly Transform _ropeStart;
        private readonly Transform _ropeBase;
        private readonly Vector3 _gripCenter;

        public SpoolProp(GameObject root, Transform wheel, Transform ropeStart, Transform ropeBase, Vector3 gripCenter)
        {
            _root = root;
            _wheel = wheel;
            _wheelRest = wheel.localRotation;
            _ropeStart = ropeStart;
            _ropeBase = ropeBase;
            _gripCenter = gripCenter;
        }

        public Vector3 RopeStart => _ropeStart.position;
        public Vector3 RopeBase => _ropeBase.position;

        public void Place(Vector3 grip, Quaternion rotation, float wheelAngle)
        {
            Transform root = _root.transform;
            root.rotation = rotation;
            root.position += grip - root.TransformPoint(_gripCenter);
            _wheel.localRotation = _wheelRest * Quaternion.Euler(0f, 0f, wheelAngle);
        }

        public void Destroy()
        {
            Object.Destroy(_root);
        }
    }
}
