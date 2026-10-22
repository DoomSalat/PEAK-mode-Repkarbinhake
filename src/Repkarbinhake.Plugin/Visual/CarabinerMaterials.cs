using System.Collections.Generic;
using Repkarbinhake.Plugin.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerMaterials
    {
        private const string GameShader = "W/Peak_Standard";
        private const string FallbackShader = "Universal Render Pipeline/Lit";
        private const string TextureProperty = "_BaseTexture";
        private const string TextureAmountProperty = "_BaseTexAmount";
        private const string VertexColorProperty = "_VertexColorAmount";
        private const string MetallicProperty = "_BaseMetallic";
        private const string SecondUvProperty = "_UseUV2";
        private const float BodyMetallic = 0.15f;
        private const float BodySmoothness = 0.35f;
        private const float BodySaturation = 1f;
        private const float GreyLimit = 0.15f;
        private const float MetalMetallic = 0.8f;
        private const float MetalSmoothness = 0.7f;
        private const float SleeveMetallic = 0f;
        private const float SleeveSmoothness = 0.35f;

        private static readonly string[] ColorProperties = { "_BaseColor", "_Color1", "_Color21", "_Color3" }; // Static: fixed shader property names, base and every layer get the same color
        private static readonly string[] SmoothProperties = { "_BaseSmooth", "_Smooth1", "_Smooth2", "_Smooth3" }; // Static: fixed shader property names
        private static readonly Color MetalColor = new Color(0.82f, 0.84f, 0.88f); // Static: fixed steel tint of the gate
        private static readonly Color BoltColor = new Color(0.2f, 0.2f, 0.22f); // Static: fixed dark tint of the hinge bolt
        private static readonly Color HookColor = new Color(0.52f, 0.54f, 0.58f); // Static: fixed plain steel of the wall hook carabiner, apart from the bright link colors

        private readonly Texture2D _knurl;
        private readonly Dictionary<Color, Material> _bodies = new Dictionary<Color, Material>();
        private Material _gate;
        private Material _sleeve;
        private Material _bolt;
        private Material _hook;

        public CarabinerMaterials(Texture2D knurl)
        {
            _knurl = knurl;
        }

        public Material[] For(Color body)
        {
            if (!_bodies.TryGetValue(body, out Material material))
            {
                material = Create(Saturate(body), BodyMetallic, BodySmoothness);
                _bodies[body] = material;
            }

            return Assemble(material);
        }

        public Material[] ForHook()
        {
            _hook = _hook ?? Create(HookColor, MetalMetallic, MetalSmoothness);
            return Assemble(_hook);
        }

        private Material[] Assemble(Material material)
        {

            _gate = _gate ?? Create(MetalColor, MetalMetallic, MetalSmoothness);
            _sleeve = _sleeve ?? CreateKnurled();
            _bolt = _bolt ?? Create(BoltColor, MetalMetallic, MetalSmoothness);
            Material[] materials = new Material[CarabinerModel.SubmeshCount];
            materials[CarabinerModel.BodySubmesh] = material;
            materials[CarabinerModel.GateSubmesh] = _gate;
            materials[CarabinerModel.SleeveSubmesh] = _sleeve;
            materials[CarabinerModel.BoltSubmesh] = _bolt;
            return materials;
        }

        private Color Saturate(Color color)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);
            return saturation < GreyLimit ? color : Color.HSVToRGB(hue, BodySaturation, value);
        }

        private Material CreateKnurled()
        {
            Material material = Create(Color.white, SleeveMetallic, SleeveSmoothness);
            material.SetTexture(TextureProperty, _knurl);
            material.SetFloat(TextureAmountProperty, 1f);
            return material;
        }

        private Material Create(Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find(GameShader);

            if (shader == null)
            {
                Debug.LogWarning($"{nameof(CarabinerMaterials)}: {GameShader} not found, using {FallbackShader}");
                return new Material(Shader.Find(FallbackShader)) { color = color };
            }

            Material material = new Material(shader);
            material.SetFloat(TextureAmountProperty, 0f);
            material.SetFloat(VertexColorProperty, 0f);
            material.SetFloat(SecondUvProperty, 0f);
            material.SetFloat(MetallicProperty, metallic);

            foreach (string property in ColorProperties)
                material.SetColor(property, color);

            foreach (string property in SmoothProperties)
                material.SetFloat(property, smoothness);

            return material;
        }
    }
}
