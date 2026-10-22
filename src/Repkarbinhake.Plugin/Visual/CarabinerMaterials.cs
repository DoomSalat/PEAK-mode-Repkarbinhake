using System.Collections.Generic;
using Repkarbinhake.Plugin.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerMaterials
    {
        private const string FallbackShader = "Universal Render Pipeline/Lit";
        private const string TextureAmountProperty = "_BaseTexAmount";
        private const string BaseColorProperty = "_BaseColor";
        private const string MetallicProperty = "_BaseMetallic";
        private const string SmoothProperty = "_BaseSmooth";
        private const float BodyMetallic = 0.3f;
        private const float MetalMetallic = 0.8f;
        private const float Smoothness = 0.6f;

        private static readonly Color MetalColor = new Color(0.72f, 0.74f, 0.78f); // Static: fixed steel tint of the gate and sleeve
        private static readonly Color BoltColor = new Color(0.2f, 0.2f, 0.22f); // Static: fixed dark tint of the hinge bolt
        private static readonly Color HookColor = new Color(0.6f, 0.62f, 0.66f); // Static: fixed body tint of the wall hook carabiner

        private readonly VanillaRopeSpool _spool;
        private readonly Dictionary<Color, Material> _bodies = new Dictionary<Color, Material>();
        private Material _metal;
        private Material _bolt;

        public CarabinerMaterials(VanillaRopeSpool spool)
        {
            _spool = spool;
        }

        public Color HookTint => HookColor;

        public Material[] For(Color body)
        {
            if (!_bodies.TryGetValue(body, out Material material))
            {
                material = Create(body, BodyMetallic);
                _bodies[body] = material;
            }

            _metal = _metal ?? Create(MetalColor, MetalMetallic);
            _bolt = _bolt ?? Create(BoltColor, MetalMetallic);
            return new[] { material, _metal, _bolt };
        }

        private Material Create(Color color, float metallic)
        {
            Material source = _spool.RopeMaterial;

            if (source == null)
                return new Material(Shader.Find(FallbackShader)) { color = color };

            Material material = new Material(source);

            if (material.HasProperty(TextureAmountProperty))
                material.SetFloat(TextureAmountProperty, 0f);

            if (material.HasProperty(MetallicProperty))
                material.SetFloat(MetallicProperty, metallic);

            if (material.HasProperty(SmoothProperty))
                material.SetFloat(SmoothProperty, Smoothness);

            material.SetColor(BaseColorProperty, color);
            return material;
        }
    }
}
