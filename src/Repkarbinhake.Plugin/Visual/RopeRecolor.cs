using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeRecolor
    {
        private const float BrightnessPull = 0.5f;

        private readonly Dictionary<Color, Material> _materials = new Dictionary<Color, Material>();

        public Material For(Material source, Color color, float strength)
        {
            if (_materials.TryGetValue(color, out Material material))
                return material;

            material = new Material(source);

            if (source.mainTexture == null)
                material.color = Color.Lerp(source.color, color, strength);
            else
                material.mainTexture = Recolor(source.mainTexture, color, strength);

            _materials.Add(color, material);
            return material;
        }

        private Texture2D Recolor(Texture source, Color color, float strength)
        {
            Color32[] pixels = Read(source);
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);
            Average(pixels, out float averageSaturation, out float averageValue);
            float brightness = Mathf.Lerp(averageValue, value, BrightnessPull) / averageValue;

            for (int i = 0; i < pixels.Length; i++)
            {
                Color original = pixels[i];
                Color.RGBToHSV(original, out _, out float pixelSaturation, out float pixelValue);
                Color painted = Color.HSVToRGB(hue, Mathf.Clamp01(pixelSaturation / averageSaturation * saturation), Mathf.Clamp01(pixelValue * brightness));
                painted.a = original.a;
                pixels[i] = Color.Lerp(original, painted, strength);
            }

            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true)
            {
                name = source.name + " " + ColorUtility.ToHtmlStringRGB(color),
                wrapMode = source.wrapMode,
                filterMode = source.filterMode,
            };
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            return texture;
        }

        private Color32[] Read(Texture source)
        {
            RenderTexture target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            Color32[] pixels = readable.GetPixels32();
            Object.Destroy(readable);
            return pixels;
        }

        private void Average(Color32[] pixels, out float saturation, out float value)
        {
            float saturationSum = 0f;
            float valueSum = 0f;

            foreach (Color32 pixel in pixels)
            {
                Color.RGBToHSV(pixel, out _, out float pixelSaturation, out float pixelValue);
                saturationSum += pixelSaturation;
                valueSum += pixelValue;
            }

            saturation = Mathf.Max(saturationSum / pixels.Length, Mathf.Epsilon);
            value = Mathf.Max(valueSum / pixels.Length, Mathf.Epsilon);
        }
    }
}
