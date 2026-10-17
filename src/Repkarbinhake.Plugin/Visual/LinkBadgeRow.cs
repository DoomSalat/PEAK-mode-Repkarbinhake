using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class LinkBadgeRow
    {
        private const string RowName = "Repkarbinhake Link Badges";
        private const string IconName = "Carabiner";
        private const float IconSize = 18f;
        private const float Spacing = 2f;
        private const float Gap = 2f;

        private static readonly Vector2 TopLeft = new Vector2(0f, 1f); // Static: constant pivot for left-to-right layout

        private readonly RectTransform _row;
        private readonly Sprite _sprite;
        private readonly Material _material;
        private readonly List<Image> _icons = new List<Image>();
        private readonly Vector3[] _corners = new Vector3[4];
        private readonly RectTransform _volume;
        private readonly RectTransform _kick;

        public LinkBadgeRow(RectTransform avatar, RectTransform volume, RectTransform kick, Sprite sprite, Material material)
        {
            _sprite = sprite;
            _material = material;
            _volume = volume;
            _kick = kick;
            GameObject row = new GameObject(RowName, typeof(RectTransform), typeof(LayoutElement));
            row.GetComponent<LayoutElement>().ignoreLayout = true;
            _row = (RectTransform)row.transform;
            _row.SetParent(avatar, false);
            _row.anchorMin = Vector2.zero;
            _row.anchorMax = Vector2.zero;
            _row.pivot = TopLeft;
            _row.anchoredPosition = new Vector2(0f, -Gap);
            _row.sizeDelta = new Vector2(0f, IconSize);
        }

        public int Count { get; private set; }
        public bool OverlapsVolume => Overlaps(_volume);
        public bool OverlapsKick => _kick != null && Overlaps(_kick);

        public void Show(IReadOnlyList<Color> colors)
        {
            Count = colors.Count;

            while (_icons.Count < colors.Count)
                _icons.Add(CreateIcon(_icons.Count));

            for (int i = 0; i < _icons.Count; i++)
            {
                bool shown = i < colors.Count;
                _icons[i].enabled = shown;

                if (shown)
                    _icons[i].color = colors[i];
            }

            _row.sizeDelta = new Vector2(colors.Count * (IconSize + Spacing), IconSize);
        }

        private bool Overlaps(RectTransform other)
        {
            return Count > 0 && other.gameObject.activeInHierarchy && WorldRect(_row).Overlaps(WorldRect(other));
        }

        private Image CreateIcon(int index)
        {
            GameObject iconObject = new GameObject(IconName, typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)iconObject.transform;
            rect.SetParent(_row, false);
            rect.anchorMin = TopLeft;
            rect.anchorMax = TopLeft;
            rect.pivot = TopLeft;
            rect.anchoredPosition = new Vector2(index * (IconSize + Spacing), 0f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = _sprite;
            icon.material = _material;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return icon;
        }

        private Rect WorldRect(RectTransform rect)
        {
            rect.GetWorldCorners(_corners);
            return Rect.MinMaxRect(_corners[0].x, _corners[0].y, _corners[2].x, _corners[2].y);
        }
    }
}
