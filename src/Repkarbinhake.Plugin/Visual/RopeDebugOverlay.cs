using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeDebugOverlay : MonoBehaviour
    {
        private const float Margin = 12f;
        private const float LineHeight = 20f;
        private const float PanelWidth = 560f;

        private readonly List<string> _lines = new List<string>();
        private bool _visible;

        private void OnGUI()
        {
            if (!_visible || _lines.Count == 0)
                return;

            GUI.Box(new Rect(Margin, Margin, PanelWidth, LineHeight * (_lines.Count + 1)), GUIContent.none);

            for (int i = 0; i < _lines.Count; i++)
                GUI.Label(new Rect(Margin * 2f, Margin + LineHeight * i, PanelWidth - Margin * 2f, LineHeight), _lines[i]);
        }

        public void Show(IReadOnlyList<string> lines)
        {
            _visible = true;
            _lines.Clear();
            _lines.AddRange(lines);
        }

        public void Hide()
        {
            _visible = false;
        }
    }
}
