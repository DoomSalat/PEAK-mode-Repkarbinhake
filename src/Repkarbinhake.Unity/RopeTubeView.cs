using System.Collections.Generic;
using UnityEngine;
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Unity
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RopeTubeView : MonoBehaviour
    {
        private const int Sides = 6;
        private const float FullTurn = Mathf.PI * 2f;
        private const float NearlyVertical = 0.9f;

        private static readonly int InteractableProperty = Shader.PropertyToID("_Interactable"); // Static: shader property id is global
        private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor"); // Static: shader property id is global

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<int> _triangles = new List<int>();
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private Color _highlightEmission;
        private float _radius;
        private int _builtRings;

        public void Initialize(MeshFilter filter, MeshRenderer meshRenderer, Material material, float radius, Color highlightEmission)
        {
            _mesh = new Mesh { name = nameof(RopeTubeView) };
            _mesh.MarkDynamic();
            filter.sharedMesh = _mesh;
            _renderer = meshRenderer;
            _renderer.sharedMaterial = material;
            _properties = new MaterialPropertyBlock();
            _highlightEmission = highlightEmission;
            _radius = radius;
        }

        public void Render(IReadOnlyList<NumericsVector3> points)
        {
            if (points.Count < 2)
                return;

            if (points.Count != _builtRings)
                RebuildTopology(points.Count);

            WriteRings(points);
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.RecalculateBounds();
        }

        public void Highlight()
        {
            ApplyHighlight(1f, _highlightEmission);
        }

        public void Unhighlight()
        {
            ApplyHighlight(0f, Color.black);
        }

        private void ApplyHighlight(float interactable, Color emission)
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetFloat(InteractableProperty, interactable);
            _properties.SetColor(EmissionColorProperty, emission);
            _renderer.SetPropertyBlock(_properties);
        }

        private void RebuildTopology(int rings)
        {
            _builtRings = rings;
            _triangles.Clear();
            _uvs.Clear();
            _vertices.Clear();
            _normals.Clear();

            for (int ring = 0; ring < rings; ring++)
            {
                for (int side = 0; side <= Sides; side++)
                {
                    _uvs.Add(new Vector2(side / (float)Sides, ring));
                    _vertices.Add(Vector3.zero);
                    _normals.Add(Vector3.up);
                }
            }

            int stride = Sides + 1;

            for (int ring = 0; ring < rings - 1; ring++)
            {
                for (int side = 0; side < Sides; side++)
                {
                    int current = ring * stride + side;
                    int next = current + stride;
                    _triangles.Add(current);
                    _triangles.Add(next);
                    _triangles.Add(current + 1);
                    _triangles.Add(current + 1);
                    _triangles.Add(next);
                    _triangles.Add(next + 1);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(_triangles, 0);
        }

        private void WriteRings(IReadOnlyList<NumericsVector3> points)
        {
            int stride = Sides + 1;
            Vector3 previousNormal = Vector3.zero;

            for (int ring = 0; ring < points.Count; ring++)
            {
                Vector3 center = transform.InverseTransformPoint(points[ring].ToUnity());
                Vector3 tangent = Tangent(points, ring);
                Vector3 normal = ring == 0 ? Perpendicular(tangent) : Vector3.ProjectOnPlane(previousNormal, tangent).normalized;

                if (normal.sqrMagnitude < Mathf.Epsilon)
                    normal = Perpendicular(tangent);

                previousNormal = normal;
                Vector3 binormal = Vector3.Cross(tangent, normal);

                for (int side = 0; side <= Sides; side++)
                {
                    float angle = side / (float)Sides * FullTurn;
                    Vector3 direction = normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle);
                    _vertices[ring * stride + side] = center + direction * _radius;
                    _normals[ring * stride + side] = direction;
                }
            }
        }

        private Vector3 Tangent(IReadOnlyList<NumericsVector3> points, int index)
        {
            int previous = Mathf.Max(index - 1, 0);
            int next = Mathf.Min(index + 1, points.Count - 1);
            Vector3 tangent = transform.InverseTransformDirection((points[next] - points[previous]).ToUnity());
            return tangent.sqrMagnitude < Mathf.Epsilon ? Vector3.up : tangent.normalized;
        }

        private Vector3 Perpendicular(Vector3 tangent)
        {
            Vector3 axis = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > NearlyVertical ? Vector3.right : Vector3.up;
            return Vector3.Cross(tangent, axis).normalized;
        }
    }
}
