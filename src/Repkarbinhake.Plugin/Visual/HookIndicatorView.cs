using Repkarbinhake.Plugin.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class HookIndicatorView
    {
        private const string IndicatorName = "Repkarbinhake Hook Indicator";
        private const string MetersName = "Repkarbinhake Hook Meters";
        private const string RetractName = "Repkarbinhake Hook Retract";
        private const float IconSize = 22f;
        private const float Offset = 26f;
        private const float MetersFontSize = 16f;
        private const float MetersWidth = 40f;
        private const float MetersHeight = 18f;

        private readonly Sprite _hookSprite;
        private readonly Sprite _retractSprite;
        private GameObject _root;
        private Image _icon;
        private Color _color;
        private TextMeshProUGUI _meters;
        private GameObject _retract;
        private int _shownMeters = -1;

        public HookIndicatorView(Sprite hookSprite, Sprite retractSprite)
        {
            _hookSprite = hookSprite;
            _retractSprite = retractSprite;
        }

        public bool Exists => _root != null;

        public void Attach(GUIManager gui)
        {
            RectTransform source = (RectTransform)gui.reticleDefault.transform;
            _root = new GameObject(IndicatorName, typeof(RectTransform));
            RectTransform rect = (RectTransform)_root.transform;
            rect.SetParent(source.parent, false);
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(-Offset, -Offset);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            _color = gui.reticleColorDefault;
            _icon = AddImage(_root, _hookSprite, gui, _color);
            CreateMeters(gui, rect);
            CreateRetract(gui, rect);
            _root.SetActive(false);
        }

        public void Show(HookIndicatorState hook)
        {
            if (hook.Placed != _root.activeSelf)
                _root.SetActive(hook.Placed);

            if (!hook.Placed)
                return;

            Color color = hook.Locked ? RepkaColors.HookLocked : hook.AtMax ? RepkaColors.HookAtMax : _color;
            _icon.color = color;
            _meters.color = color;
            int meters = Mathf.RoundToInt(hook.Meters);

            if (meters != _shownMeters)
            {
                _shownMeters = meters;
                _meters.text = meters.ToString();
            }

            bool retracting = hook.Locked && hook.Retracting;

            if (retracting != _retract.activeSelf)
                _retract.SetActive(retracting);
        }

        private void CreateMeters(GUIManager gui, RectTransform parent)
        {
            GameObject meters = new GameObject(MetersName, typeof(RectTransform));
            RectTransform rect = (RectTransform)meters.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.down * (IconSize + MetersHeight) / 2f;
            rect.sizeDelta = new Vector2(MetersWidth, MetersHeight);
            _meters = meters.AddComponent<TextMeshProUGUI>();
            _meters.font = gui.interactNameText.font;
            _meters.fontSharedMaterial = gui.interactNameText.fontSharedMaterial;
            _meters.fontSize = MetersFontSize;
            _meters.alignment = TextAlignmentOptions.Center;
            _meters.textWrappingMode = TextWrappingModes.NoWrap;
            _meters.raycastTarget = false;
        }

        private void CreateRetract(GUIManager gui, RectTransform parent)
        {
            _retract = new GameObject(RetractName, typeof(RectTransform));
            RectTransform rect = (RectTransform)_retract.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.left * IconSize;
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            AddImage(_retract, _retractSprite, gui, RepkaColors.HookLocked);
            _retract.SetActive(false);
        }

        private Image AddImage(GameObject target, Sprite sprite, GUIManager gui, Color color)
        {
            Image image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.material = gui.reticleDefaultImage.material;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }
    }
}
