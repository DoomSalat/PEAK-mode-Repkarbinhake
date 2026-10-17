using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerHud
    {
        private const string ReticleName = "Repkarbinhake Carabiner Reticle";
        private const string PromptName = "Repkarbinhake Link Prompt";
        private const string PartnerName = "Repkarbinhake Rope Partner";
        private const float ReticleSize = 48f;
        private const string KeyFallbackFormat = "[{0}]";

        private readonly Sprite _sprite;
        private readonly ManualLogSource _log;
        private GameObject _reticle;
        private GameObject _prompt;
        private TMP_Text _promptText;
        private TMP_Text _promptKey;
        private GameObject _vanillaPrompt;
        private TMP_Text _partner;
        private Vector2 _promptHome;
        private string _keyPath;
        private string _keyTag;

        public CarabinerHud(Sprite sprite, ManualLogSource log)
        {
            _sprite = sprite;
            _log = log;
        }

        public void Attach(GUIManager gui)
        {
            CreateReticle(gui);
            CreatePrompt(gui);
            CreatePartner(gui);
        }

        public void Show(GUIManager gui, bool showReticle, string prompt, string keyPath, string partner)
        {
            if (_reticle == null || _prompt == null || _partner == null)
                return;

            if (showReticle)
                gui.SetReticle(_reticle);

            ShowPrompt(prompt, keyPath);
            ShowPartner(partner);
        }

        private void CreateReticle(GUIManager gui)
        {
            RectTransform source = (RectTransform)gui.reticleDefault.transform;
            _reticle = new GameObject(ReticleName, typeof(RectTransform));
            RectTransform rect = (RectTransform)_reticle.transform;
            rect.SetParent(source.parent, false);
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.anchoredPosition = source.anchoredPosition;
            rect.sizeDelta = new Vector2(ReticleSize, ReticleSize);
            Image icon = _reticle.AddComponent<Image>();
            icon.sprite = _sprite;
            icon.material = gui.reticleDefaultImage.material;
            icon.color = gui.reticleColorDefault;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            _reticle.SetActive(false);
            gui.reticleList.Add(_reticle);

            if (_sprite == null)
                _log.LogWarning($"{nameof(CarabinerHud)}: carabiner sprite is missing, reticle will be blank");
        }

        private void CreatePrompt(GUIManager gui)
        {
            _vanillaPrompt = gui.interactPromptPrimary;
            _prompt = Object.Instantiate(_vanillaPrompt, _vanillaPrompt.transform.parent);
            _prompt.name = PromptName;
            _promptHome = ((RectTransform)_prompt.transform).anchoredPosition;
            _promptKey = null;
            _promptText = null;

            foreach (InputIcon icon in _prompt.GetComponentsInChildren<InputIcon>(true))
            {
                _promptKey = icon.GetComponent<TMP_Text>();
                Object.DestroyImmediate(icon);
            }

            foreach (LocalizedText localized in _prompt.GetComponentsInChildren<LocalizedText>(true))
                Object.DestroyImmediate(localized);

            foreach (TMP_Text text in _prompt.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text != _promptKey)
                {
                    _promptText = text;
                    break;
                }
            }

            _prompt.SetActive(false);

            if (_promptKey == null || _promptText == null)
                _log.LogWarning($"{nameof(CarabinerHud)}: interact prompt layout changed (key {_promptKey != null}, text {_promptText != null}), link prompt disabled");
        }

        private void CreatePartner(GUIManager gui)
        {
            _partner = Object.Instantiate(gui.interactNameText, _vanillaPrompt.transform.parent);
            _partner.name = PartnerName;

            foreach (LocalizedText localized in _partner.GetComponentsInChildren<LocalizedText>(true))
                Object.DestroyImmediate(localized);

            RectTransform source = (RectTransform)_vanillaPrompt.transform;
            RectTransform rect = _partner.rectTransform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            _partner.alignment = TextAlignmentOptions.Center;
            _partner.raycastTarget = false;
            _partner.gameObject.SetActive(false);
        }

        private void ShowPartner(string partner)
        {
            if (string.IsNullOrEmpty(partner) || !_vanillaPrompt.activeSelf)
            {
                _partner.gameObject.SetActive(false);
                return;
            }

            RectTransform source = (RectTransform)_vanillaPrompt.transform;
            _partner.rectTransform.anchoredPosition = source.anchoredPosition + Vector2.down * source.rect.height;
            _partner.text = partner;
            _partner.gameObject.SetActive(true);
        }

        private void ShowPrompt(string text, string keyPath)
        {
            if (text == null || _promptKey == null || _promptText == null)
            {
                _prompt.SetActive(false);
                return;
            }

            RectTransform rect = (RectTransform)_prompt.transform;
            rect.anchoredPosition = _vanillaPrompt.activeSelf ? _promptHome + Vector2.up * rect.rect.height : _promptHome;
            _promptKey.spriteAsset = SingletonAsset<InputSpriteData>.Instance.keyboardSprites;
            _promptKey.text = KeyTag(keyPath);
            _promptText.text = text;
            _prompt.SetActive(true);
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
