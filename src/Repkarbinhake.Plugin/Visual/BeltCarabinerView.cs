using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class BeltCarabinerView
    {
        private const string ObjectName = "Repkarbinhake Belt Carabiner";

        private readonly GameObject _root;
        private readonly MeshRenderer _renderer;

        public BeltCarabinerView(Transform parent, Mesh mesh, float scale)
        {
            _root = new GameObject(ObjectName);
            _root.transform.SetParent(parent, false);
            _root.transform.localScale = Vector3.one * scale;
            _root.AddComponent<MeshFilter>().sharedMesh = mesh;
            _renderer = _root.AddComponent<MeshRenderer>();
        }

        public void Paint(Material[] materials)
        {
            _renderer.sharedMaterials = materials;
        }

        public void Place(Vector3 pivot, Quaternion rotation)
        {
            _root.transform.SetPositionAndRotation(pivot, rotation);
        }

        public void Show()
        {
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        public void Destroy()
        {
            Object.Destroy(_root);
        }
    }
}
