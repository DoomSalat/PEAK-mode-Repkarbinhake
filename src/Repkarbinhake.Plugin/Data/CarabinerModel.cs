using UnityEngine;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class CarabinerModel
    {
        public const int BodySubmesh = 0;
        public const int MetalSubmesh = 1;
        public const int BoltSubmesh = 2;

        private const float TubeRadius = 0.0092f;

        public CarabinerModel(Mesh mesh)
        {
            Bounds bounds = mesh.bounds;
            Vector3 pivot = new Vector3(bounds.center.x, bounds.max.y - TubeRadius, bounds.center.z);
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= pivot;

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            Mesh = mesh;
            HangLength = pivot.y - bounds.min.y - TubeRadius;
        }

        public Mesh Mesh { get; }
        public float HangLength { get; }
    }
}
