using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeRecolor
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor"); // Static: shader property id is global
        private static readonly int[] AccentProperties = { Shader.PropertyToID("_Color1"), Shader.PropertyToID("_Color21"), Shader.PropertyToID("_Color3") }; // Static: shader property ids are global

        private readonly Dictionary<Color, Material> _materials = new Dictionary<Color, Material>();

        public Material For(Material source, Color color, float strength)
        {
            if (_materials.TryGetValue(color, out Material material))
                return material;

            material = new Material(source);
            Color.RGBToHSV(color, out float hue, out float saturation, out _);

            if (material.HasProperty(BaseColorProperty))
                Paint(material, BaseColorProperty, hue, saturation, strength);

            foreach (int property in AccentProperties)
            {
                if (material.HasProperty(property))
                    Paint(material, property, hue, null, strength);
            }

            _materials.Add(color, material);
            return material;
        }

        private void Paint(Material material, int property, float hue, float? saturation, float strength)
        {
            Color original = material.GetColor(property);
            Color.RGBToHSV(original, out _, out float originalSaturation, out float value);
            Color painted = Color.HSVToRGB(hue, saturation ?? originalSaturation, value, true);
            painted.a = original.a;
            material.SetColor(property, Color.Lerp(original, painted, strength));
        }
    }
}
