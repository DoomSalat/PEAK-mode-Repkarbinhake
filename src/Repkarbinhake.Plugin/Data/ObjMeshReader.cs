using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class ObjMeshReader
    {
        private const char Separator = ' ';
        private const char IndexSeparator = '/';
        private const string PositionTag = "v";
        private const string TexcoordTag = "vt";
        private const string NormalTag = "vn";
        private const string FaceTag = "f";
        private const string MaterialTag = "usemtl";
        private const int TriangleCorners = 3;

        public Mesh Read(string text, string name, IReadOnlyList<string[]> submeshMaterials)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> vertexNormals = new List<Vector3>();
            Dictionary<string, int> corners = new Dictionary<string, int>();
            List<int>[] triangles = new List<int>[submeshMaterials.Count];

            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = new List<int>();

            int submesh = submeshMaterials.Count - 1;

            using (StringReader reader = new StringReader(text))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 0)
                        continue;

                    switch (parts[0])
                    {
                        case PositionTag:
                            positions.Add(new Vector3(-Number(parts[1]), Number(parts[2]), Number(parts[3])));
                            break;

                        case TexcoordTag:
                            texcoords.Add(new Vector2(Number(parts[1]), Number(parts[2])));
                            break;

                        case NormalTag:
                            normals.Add(new Vector3(-Number(parts[1]), Number(parts[2]), Number(parts[3])));
                            break;

                        case MaterialTag:
                            submesh = SubmeshOf(parts[1], submeshMaterials);
                            break;

                        case FaceTag:
                            AddFace(parts, triangles[submesh], corners, positions, texcoords, normals, vertices, uvs, vertexNormals);
                            break;
                    }
                }
            }

            Mesh mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(vertexNormals);
            mesh.subMeshCount = triangles.Length;

            for (int i = 0; i < triangles.Length; i++)
                mesh.SetTriangles(triangles[i], i);

            mesh.RecalculateBounds();
            return mesh;
        }

        private void AddFace(string[] parts, List<int> triangles, Dictionary<string, int> corners, List<Vector3> positions, List<Vector2> texcoords, List<Vector3> normals, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> vertexNormals)
        {
            if (parts.Length != TriangleCorners + 1)
                throw new InvalidDataException($"{nameof(ObjMeshReader)}: only triangles are supported, got {parts.Length - 1} corners");

            int first = Corner(parts[1], corners, positions, texcoords, normals, vertices, uvs, vertexNormals);
            int second = Corner(parts[2], corners, positions, texcoords, normals, vertices, uvs, vertexNormals);
            int third = Corner(parts[3], corners, positions, texcoords, normals, vertices, uvs, vertexNormals);
            triangles.Add(first);
            triangles.Add(third);
            triangles.Add(second);
        }

        private int Corner(string corner, Dictionary<string, int> corners, List<Vector3> positions, List<Vector2> texcoords, List<Vector3> normals, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> vertexNormals)
        {
            if (corners.TryGetValue(corner, out int index))
                return index;

            string[] ids = corner.Split(IndexSeparator);
            index = vertices.Count;
            vertices.Add(positions[int.Parse(ids[0], CultureInfo.InvariantCulture) - 1]);
            uvs.Add(ids.Length > 1 && ids[1].Length > 0 ? texcoords[int.Parse(ids[1], CultureInfo.InvariantCulture) - 1] : Vector2.zero);
            vertexNormals.Add(ids.Length > 2 && ids[2].Length > 0 ? normals[int.Parse(ids[2], CultureInfo.InvariantCulture) - 1] : Vector3.up);
            corners[corner] = index;
            return index;
        }

        private int SubmeshOf(string material, IReadOnlyList<string[]> submeshMaterials)
        {
            for (int i = 0; i < submeshMaterials.Count; i++)
            {
                if (Array.IndexOf(submeshMaterials[i], material) >= 0)
                    return i;
            }

            return submeshMaterials.Count - 1;
        }

        private float Number(string text)
        {
            return float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
