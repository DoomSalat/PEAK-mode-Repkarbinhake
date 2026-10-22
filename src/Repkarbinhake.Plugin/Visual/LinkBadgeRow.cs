using System.Collections.Generic;
using Repkarbinhake.Plugin.Links;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class LinkBadgeRow
    {
        private const string RowName = "Repkarbinhake Link Badges";
        private const string IconName = "Carabiner";
        private const string NumberName = "Number";
        private const float NumberFontSize = 17f;
        private const float NumberMinFontSize = 9f;
        private const float NumberOutline = 0.1f;
        private const float IconSize = 18f;
        private const float Spacing = 2f;
        private const float Gap = 8f;

        private static readonly Vector2 TopLeft = new Vector2(0f, 1f); // Static: constant pivot for left-to-right layout
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f); // Static: constant pivot an icon flips around

        private readonly RectTransform _row;
        private readonly Sprite _sprite;
        private readonly Material _material;
        private readonly TMP_Text _font;
        private readonly List<Image> _icons = new List<Image>();
        private readonly List<TextMeshProUGUI> _numbers = new List<TextMeshProUGUI>();
        private readonly List<int> _shownNumbers = new List<int>();
        private readonly Vector3[] _corners = new Vector3[4];
        private readonly RectTransform _volume;
        private readonly RectTransform _kick;

        public LinkBadgeRow(RectTransform avatar, RectTransform volume, RectTransform kick, Sprite sprite, Material material, TMP_Text font)
        {
            _sprite = sprite;
            _material = material;
            _font = font;
            _volume = volume;
            _kick = kick;
            GameObject row = new GameObject(RowName, typeof(RectTransform));
            row.AddComponent<LayoutElement>().ignoreLayout = true;
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

        public void Show(IReadOnlyList<LinkBadge> badges)
        {
            Count = badges.Count;

            while (_icons.Count < badges.Count)
            {
                Image icon = CreateIcon(_icons.Count);
                _icons.Add(icon);
                _numbers.Add(CreateNumber(icon.rectTransform));
                _shownNumbers.Add(0);
            }

            for (int i = 0; i < _icons.Count; i++)
            {
                bool shown = i < badges.Count;
                _icons[i].enabled = shown;
                _numbers[i].enabled = shown;

                if (!shown)
                    continue;

                _icons[i].color = badges[i].Color;
                _numbers[i].color = badges[i].Color;

                if (_shownNumbers[i] != badges[i].Number)
                {
                    _shownNumbers[i] = badges[i].Number;
                    _numbers[i].text = badges[i].Number.ToString();
                }
            }

            _row.sizeDelta = new Vector2(badges.Count * (IconSize + Spacing), IconSize);
        }

        private bool Overlaps(RectTransform other)
        {
            return Count > 0 && other.gameObject.activeInHierarchy && WorldRect(_row).Overlaps(WorldRect(other));
        }

        private Image CreateIcon(int index)
        {
            GameObject iconObject = new GameObject(IconName, typeof(RectTransform));
            RectTransform rect = (RectTransform)iconObject.transform;
            rect.SetParent(_row, false);
            rect.anchorMin = TopLeft;
            rect.anchorMax = TopLeft;
            rect.pivot = Center;
            rect.anchoredPosition = new Vector2(index * (IconSize + Spacing) + IconSize / 2f, -IconSize / 2f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            Vector3 rowScale = _row.lossyScale;
            // Undoes the mirrored avatar parent around the icon center, so it stays in place and reads the right way
            rect.localScale = new Vector3(Mathf.Sign(rowScale.x), Mathf.Sign(rowScale.y), 1f);
            Image icon = iconObject.AddComponent<Image>();
            icon.sprite = _sprite;
            icon.material = _material;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return icon;
        }

        private TextMeshProUGUI CreateNumber(RectTransform icon)
        {
            GameObject numberObject = new GameObject(NumberName, typeof(RectTransform));
            RectTransform rect = (RectTransform)numberObject.transform;
            rect.SetParent(icon, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            TextMeshProUGUI number = numberObject.AddComponent<TextMeshProUGUI>();
            number.margin = Vector4.zero;
            number.font = _font.font;
            number.fontSharedMaterial = _font.fontSharedMaterial;
            number.enableAutoSizing = true;
            number.fontSizeMax = NumberFontSize;
            number.fontSizeMin = NumberMinFontSize;
            Material outlined = number.fontMaterial;
            outlined.EnableKeyword(ShaderUtilities.Keyword_Outline);
            outlined.SetFloat(ShaderUtilities.ID_OutlineWidth, NumberOutline);
            outlined.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            number.alignment = TextAlignmentOptions.Midline;
            number.textWrappingMode = TextWrappingModes.NoWrap;
            number.raycastTarget = false;
            return number;
        }

        private Rect WorldRect(RectTransform rect)
        {
            rect.GetWorldCorners(_corners);
            return Rect.MinMaxRect(_corners[0].x, _corners[0].y, _corners[2].x, _corners[2].y);
        }
    }
}
