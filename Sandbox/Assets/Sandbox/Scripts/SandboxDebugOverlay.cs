using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxDebugOverlay : MonoBehaviour
    {
        private const float Margin = 12f;
        private const float LineHeight = 20f;
        private const float PanelWidth = 520f;
        private const float DotSize = 6f;
        private const int RingDots = 40;
        private const float RingDotSize = 5f;
        private const float RingGap = 8f;

        private const string Help =
            "1-4 выбрать игрока | WASD идти | Space прыжок | мышь камера | Esc курсор\n" +
            "Держать G на игроке — связаться / отцепиться | E на верёвке — взять/отпустить (не на верёвке — отпустить все)\n" +
            "Колесо смотать/размотать держимые | F упасть | U без сознания | K убить | L выйти из игры | R вернуть всех на старт | P столкнуть цель | T сцена «скала»";

        [SerializeField] private Texture2D _reticleIcon;
        [SerializeField, Min(8f)] private float _reticleSize = 48f;
        [SerializeField] private Color _reticleColor = Color.white;
        [SerializeField] private Color _dotColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _ringFillColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color _ringEmptyColor = new Color(1f, 1f, 1f, 0.2f);

        private readonly List<string> _lines = new List<string>();
        private bool _showCarabiner;
        private float _holdProgress;
        private string _aimHint = string.Empty;
        private GUIStyle _centeredLabel;

        private void OnGUI()
        {
            DrawPanel();
            DrawReticle();
        }

        public void Show(IReadOnlyList<string> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
        }

        public void ShowCarabiner(string hint, float holdProgress)
        {
            _showCarabiner = true;
            _aimHint = hint;
            _holdProgress = holdProgress;
        }

        public void ShowHint(string hint)
        {
            _showCarabiner = false;
            _aimHint = hint;
            _holdProgress = 0f;
        }

        public void HideCarabiner()
        {
            _showCarabiner = false;
            _aimHint = string.Empty;
            _holdProgress = 0f;
        }

        private void DrawPanel()
        {
            float height = LineHeight * (_lines.Count + 4);
            GUI.Box(new Rect(Margin, Margin, PanelWidth, height), GUIContent.none);
            GUI.Label(new Rect(Margin * 2f, Margin, PanelWidth - Margin * 2f, LineHeight * 3f), Help);

            for (int i = 0; i < _lines.Count; i++)
                GUI.Label(new Rect(Margin * 2f, Margin + LineHeight * (i + 3), PanelWidth - Margin * 2f, LineHeight), _lines[i]);
        }

        private void DrawReticle()
        {
            Vector2 center = new Vector2(Screen.width, Screen.height) / 2f;
            Color previous = GUI.color;

            if (!_showCarabiner)
            {
                GUI.color = _dotColor;
                GUI.DrawTexture(new Rect(center.x - DotSize / 2f, center.y - DotSize / 2f, DotSize, DotSize), Texture2D.whiteTexture);
                GUI.color = previous;
                GUI.Label(new Rect(center.x - PanelWidth / 2f, center.y + DotSize + RingGap, PanelWidth, LineHeight), _aimHint, CenteredLabel());
                return;
            }

            GUI.color = _reticleColor;
            GUI.DrawTexture(new Rect(center.x - _reticleSize / 2f, center.y - _reticleSize / 2f, _reticleSize, _reticleSize), _reticleIcon, ScaleMode.ScaleToFit);
            DrawHoldRing(center);
            GUI.color = previous;
            GUI.Label(new Rect(center.x - PanelWidth / 2f, center.y + _reticleSize / 2f + RingGap, PanelWidth, LineHeight), _aimHint, CenteredLabel());
        }

        private void DrawHoldRing(Vector2 center)
        {
            if (_holdProgress <= 0f)
                return;

            float radius = _reticleSize / 2f + RingGap;
            int filled = Mathf.CeilToInt(RingDots * _holdProgress);

            for (int i = 0; i < RingDots; i++)
            {
                float angle = i / (float)RingDots * Mathf.PI * 2f - Mathf.PI / 2f;
                Vector2 dot = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                GUI.color = i < filled ? _ringFillColor : _ringEmptyColor;
                GUI.DrawTexture(new Rect(dot.x - RingDotSize / 2f, dot.y - RingDotSize / 2f, RingDotSize, RingDotSize), Texture2D.whiteTexture);
            }
        }

        private GUIStyle CenteredLabel()
        {
            if (_centeredLabel == null)
                _centeredLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter };

            return _centeredLabel;
        }
    }
}
