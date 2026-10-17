using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeGaugeTint
    {
        private const string GaugeShader = "Scouts/UI_Rope";

        private readonly List<Image> _images = new List<Image>();
        private readonly List<Color> _originals = new List<Color>();
        private UI_Rope _rope;
        private bool _painted;

        public void Paint(UI_Rope rope, Color color, float strength)
        {
            Collect(rope);

            for (int i = 0; i < _images.Count; i++)
                Apply(_images[i], Color.Lerp(_originals[i], color, strength));

            _painted = true;
        }

        public void Restore(UI_Rope rope)
        {
            if (!_painted)
                return;

            Collect(rope);

            for (int i = 0; i < _images.Count; i++)
                Apply(_images[i], _originals[i]);

            _painted = false;
        }

        private void Collect(UI_Rope rope)
        {
            if (_rope == rope)
                return;

            _rope = rope;
            _images.Clear();
            _originals.Clear();

            foreach (Image image in rope.GetComponentsInChildren<Image>(true))
            {
                if (image.material.shader.name != GaugeShader)
                    continue;

                _images.Add(image);
                _originals.Add(image.color);
            }
        }

        private void Apply(Image image, Color color)
        {
            image.color = new Color(color.r, color.g, color.b, image.color.a);
        }
    }
}
