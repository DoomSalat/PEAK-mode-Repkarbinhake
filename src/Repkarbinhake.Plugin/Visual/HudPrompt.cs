using System.Collections.Generic;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class HudPrompt
    {
        private const string KeyFallbackFormat = "[{0}]";

        private readonly GameObject _root;
        private readonly RectTransform _rect;
        private readonly TMP_Text _text;
        private readonly TMP_Text _key;
        private readonly Dictionary<string, string> _keyTags = new Dictionary<string, string>();

        public HudPrompt(GameObject vanilla, string name, ManualLogSource log)
        {
            _root = Object.Instantiate(vanilla, vanilla.transform.parent);
            _root.name = name;
            _rect = (RectTransform)_root.transform;

            foreach (InputIcon icon in _root.GetComponentsInChildren<InputIcon>(true))
            {
                _key = icon.GetComponent<TMP_Text>();
                Object.DestroyImmediate(icon);
            }

            foreach (LocalizedText localized in _root.GetComponentsInChildren<LocalizedText>(true))
                Object.DestroyImmediate(localized);

            foreach (TMP_Text text in _root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text != _key && text.gameObject.activeSelf)
                {
                    _text = text;
                    break;
                }
            }

            _root.SetActive(false);

            if (_key == null || _text == null)
                log.LogWarning($"{nameof(HudPrompt)} {name}: interact prompt layout changed (key {_key != null}, text {_text != null}), prompt disabled");
        }

        public bool Exists => _root != null;
        public bool IsComplete => _key != null && _text != null;

        public void Show(HudPromptLine line)
        {
            if (!IsComplete || line.Text == null)
            {
                Hide();
                return;
            }

            _key.spriteAsset = SingletonAsset<InputSpriteData>.Instance.keyboardSprites;
            _key.text = KeyTag(line.KeyPath);
            _text.text = line.Text;
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        public void LeaveLayout()
        {
            _root.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        public void PlaceRightOf(HudPrompt other, float gap)
        {
            if (IsComplete && other.IsComplete)
                HudPlacement.RightOf(other._rect, other._text, _rect, _key, gap);
        }

        private string KeyTag(string path)
        {
            if (_keyTags.TryGetValue(path, out string cached))
                return cached;

            string tag = SingletonAsset<InputSpriteData>.Instance.GetSpriteTagFromInputPathKeyboard(path);

            if (string.IsNullOrEmpty(tag))
                tag = string.Format(KeyFallbackFormat, InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice));

            _keyTags[path] = tag;
            return tag;
        }
    }
}
