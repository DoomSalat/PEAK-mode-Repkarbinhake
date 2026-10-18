using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerHud
    {
        private const string ReticleName = "Repkarbinhake Carabiner Reticle";
        private const string PromptName = "Repkarbinhake Link Prompt";
        private const string PullPromptName = "Repkarbinhake Pull Prompt";
        private const string PartnerName = "Repkarbinhake Rope Partner";
        private const string HookIndicatorName = "Repkarbinhake Hook Indicator";
        private const float HookIndicatorSize = 22f;
        private const float HookIndicatorOffset = 26f;
        private const float ReticleSize = 48f;

        private readonly Sprite _sprite;
        private readonly Sprite _hookSprite;
        private readonly ManualLogSource _log;
        private GameObject _reticle;
        private HudPrompt _prompt;
        private HudPrompt _pullPrompt;
        private GameObject _vanillaPrompt;
        private TMP_Text _partner;
        private GameObject _hookIndicator;

        public CarabinerHud(Sprite sprite, Sprite hookSprite, ManualLogSource log)
        {
            _sprite = sprite;
            _hookSprite = hookSprite;
            _log = log;
        }

        public void Attach(GUIManager gui)
        {
            CreateReticle(gui);
            _vanillaPrompt = gui.interactPromptPrimary;
            _prompt = new HudPrompt(_vanillaPrompt, PromptName, _log);
            _pullPrompt = new HudPrompt(_vanillaPrompt, PullPromptName, _log);
            CreatePartner(gui);
            CreateHookIndicator(gui);
        }

        public void Show(GUIManager gui, bool showReticle, HudPromptLine prompt, HudPromptLine pullPrompt, string partner, Color partnerColor, bool hookPlaced)
        {
            if (_reticle == null || !_prompt.Exists || !_pullPrompt.Exists || _partner == null || _hookIndicator == null)
                return;

            if (hookPlaced != _hookIndicator.activeSelf)
                _hookIndicator.SetActive(hookPlaced);

            if (showReticle)
                gui.SetReticle(_reticle);

            int row = _vanillaPrompt.activeSelf ? 1 : 0;
            row = ShowLine(_prompt, prompt, row);
            ShowLine(_pullPrompt, pullPrompt, row);
            ShowPartner(partner, partnerColor);
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


        private void CreateHookIndicator(GUIManager gui)
        {
            RectTransform source = (RectTransform)gui.reticleDefault.transform;
            _hookIndicator = new GameObject(HookIndicatorName, typeof(RectTransform));
            RectTransform rect = (RectTransform)_hookIndicator.transform;
            rect.SetParent(source.parent, false);
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(-HookIndicatorOffset, -HookIndicatorOffset);
            rect.sizeDelta = new Vector2(HookIndicatorSize, HookIndicatorSize);
            Image icon = _hookIndicator.AddComponent<Image>();
            icon.sprite = _hookSprite;
            icon.material = gui.reticleDefaultImage.material;
            icon.color = gui.reticleColorDefault;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            _hookIndicator.SetActive(false);
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

        private void ShowPartner(string partner, Color color)
        {
            if (string.IsNullOrEmpty(partner) || !_vanillaPrompt.activeSelf)
            {
                _partner.gameObject.SetActive(false);
                return;
            }

            RectTransform source = (RectTransform)_vanillaPrompt.transform;
            _partner.rectTransform.anchoredPosition = source.anchoredPosition + Vector2.down * source.rect.height;
            _partner.text = partner;
            _partner.color = color;
            _partner.gameObject.SetActive(true);
        }

        private int ShowLine(HudPrompt prompt, HudPromptLine line, int row)
        {
            if (line.Text == null)
            {
                prompt.Hide();
                return row;
            }

            prompt.Show(line.Text, line.KeyPath, row);
            return row + 1;
        }
    }
}
