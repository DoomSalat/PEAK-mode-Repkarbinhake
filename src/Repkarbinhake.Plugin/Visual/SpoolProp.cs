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
        [SerializeField] private MeshRenderer[] _renderers;
        [SerializeField] private Material _source;

        private Color? _paintedColor;

        public Vector3 RopeStart => _ropeStart.position;
        public Vector3 RopeBase => _ropeBase.position;
        public Material Source => _source;

        public void Initialize(Transform wheel, Transform ropeStart, Transform ropeBase, Vector3 gripCenter, MeshRenderer[] renderers)
        {
            _wheel = wheel;
            _ropeStart = ropeStart;
            _ropeBase = ropeBase;
            _gripCenter = gripCenter;
            _wheelRest = wheel.localRotation;
            _renderers = renderers;
            _source = renderers[0].sharedMaterial;
        }

        public void Place(Vector3 grip, Quaternion rotation, float wheelAngle)
        {
            transform.rotation = rotation;
            transform.position += grip - transform.TransformPoint(_gripCenter);
            _wheel.localRotation = _wheelRest * Quaternion.Euler(0f, 0f, wheelAngle);
        }

        public bool IsPainted(Color color)
        {
            return _paintedColor == color;
        }

        public void Paint(Color color, Material material)
        {
            _paintedColor = color;
            ApplyMaterial(material);
        }

        public void Unpaint()
        {
            if (_paintedColor == null)
                return;

            _paintedColor = null;
            ApplyMaterial(_source);
        }

        private void ApplyMaterial(Material material)
        {
            foreach (MeshRenderer meshRenderer in _renderers)
                meshRenderer.sharedMaterial = material;
        }
    }
}
