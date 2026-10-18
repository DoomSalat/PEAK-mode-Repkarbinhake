using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class HudPrompt
    {
        private const string KeyFallbackFormat = "[{0}]";

        private readonly GameObject _root;
        private readonly RectTransform _rect;
        private readonly Vector2 _home;
        private readonly TMP_Text _text;
        private readonly TMP_Text _key;
        private string _keyPath;
        private string _keyTag;

        public HudPrompt(GameObject vanilla, string name, ManualLogSource log)
        {
            _root = Object.Instantiate(vanilla, vanilla.transform.parent);
            _root.name = name;
            _rect = (RectTransform)_root.transform;
            _home = _rect.anchoredPosition;

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

        public void Show(string text, string keyPath, int row)
        {
            if (_key == null || _text == null)
                return;

            _rect.anchoredPosition = _home + Vector2.up * _rect.rect.height * row;
            _key.spriteAsset = SingletonAsset<InputSpriteData>.Instance.keyboardSprites;
            _key.text = KeyTag(keyPath);
            _text.text = text;
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private string KeyTag(string path)
        {
            if (path == _keyPath)
                return _keyTag;

            _keyPath = path;
            _keyTag = SingletonAsset<InputSpriteData>.Instance.GetSpriteTagFromInputPathKeyboard(path);

            if (string.IsNullOrEmpty(_keyTag))
                _keyTag = string.Format(KeyFallbackFormat, InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice));

            return _keyTag;
        }
    }
}
