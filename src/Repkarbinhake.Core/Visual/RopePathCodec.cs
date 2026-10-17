using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopePathCodec
    {
        private const int Axes = 3;

        public float[] Encode(IReadOnlyList<Vector3> points, IReadOnlyList<float> materials)
        {
            int contacts = points.Count - 2;
            float[] data = new float[1 + contacts * Axes + materials.Count];
            data[0] = contacts;
            int index = 1;

            for (int i = 1; i <= contacts; i++)
            {
                data[index++] = points[i].X;
                data[index++] = points[i].Y;
                data[index++] = points[i].Z;
            }

            foreach (float material in materials)
                data[index++] = material;

            return data;
        }

        public bool TryDecode(float[] data, List<Vector3> contacts, List<float> materials)
        {
            contacts.Clear();
            materials.Clear();

            if (data == null || data.Length < 1)
                return false;

            int count = (int)data[0];

            if (count < 0 || count > RopeWrapPath.MaxContacts || data.Length != 1 + count * Axes + count + 1)
                return false;

            int index = 1;

            for (int i = 0; i < count; i++)
            {
                contacts.Add(new Vector3(data[index], data[index + 1], data[index + 2]));
                index += Axes;
            }

            for (int i = 0; i <= count; i++)
                materials.Add(data[index++]);

            return true;
        }
    }
}
