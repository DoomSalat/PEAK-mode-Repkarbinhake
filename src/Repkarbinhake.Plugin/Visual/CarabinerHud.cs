using BepInEx.Logging;
using System.Reflection;
using HarmonyLib;
using Repkarbinhake.Plugin.Data;
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
        private const string HookMetersName = "Repkarbinhake Hook Meters";
        private const string HookRetractName = "Repkarbinhake Hook Retract";
        private const float HookIndicatorSize = 22f;
        private const float HookIndicatorOffset = 26f;
        private const float HookMetersFontSize = 16f;
        private const float HookMetersWidth = 40f;
        private const float HookMetersHeight = 18f;
        private const float ReticleSize = 48f;
        private const string PulseEaseField = "easeType";

        private readonly Sprite _sprite;
        private readonly Sprite _hookSprite;
        private readonly Sprite _retractSprite;
        private readonly ManualLogSource _log;
        private GameObject _reticle;
        private HudPrompt _prompt;
        private HudPrompt _pullPrompt;
        private GameObject _vanillaPrompt;
        private TMP_Text _partner;
        private GameObject _hookIndicator;
        private Image _hookIcon;
        private Color _hookColor;
        private TextMeshProUGUI _hookMeters;
        private GameObject _hookRetract;
        private int _shownMeters = -1;

        public CarabinerHud(Sprite sprite, Sprite hookSprite, Sprite retractSprite, ManualLogSource log)
        {
            _sprite = sprite;
            _hookSprite = hookSprite;
            _retractSprite = retractSprite;
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

        public void Show(GUIManager gui, bool showReticle, HudPromptLine prompt, HudPromptLine pullPrompt, HudPromptLine unwindPrompt, string partner, Color partnerColor, HookIndicatorState hook)
        {
            if (_reticle == null || !_prompt.Exists || !_pullPrompt.Exists || _partner == null || _hookIndicator == null)
                return;

            ShowHook(hook);

            if (showReticle)
                gui.SetReticle(_reticle);

            int row = _vanillaPrompt.activeSelf ? 1 : 0;
            row = ShowLine(_prompt, prompt, row);
            ShowLine(_pullPrompt, pullPrompt, unwindPrompt, row);
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
            CopyPulse(gui.reticleRope, _reticle);
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
            _hookIcon = icon;
            _hookColor = gui.reticleColorDefault;
            icon.sprite = _hookSprite;
            icon.material = gui.reticleDefaultImage.material;
            icon.color = gui.reticleColorDefault;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            CreateHookMeters(gui, rect);
            CreateHookRetract(gui, rect);
            _hookIndicator.SetActive(false);
        }

        private void CreateHookMeters(GUIManager gui, RectTransform parent)
        {
            GameObject meters = new GameObject(HookMetersName, typeof(RectTransform));
            RectTransform rect = (RectTransform)meters.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.down * (HookIndicatorSize + HookMetersHeight) / 2f;
            rect.sizeDelta = new Vector2(HookMetersWidth, HookMetersHeight);
            _hookMeters = meters.AddComponent<TextMeshProUGUI>();
            _hookMeters.font = gui.interactNameText.font;
            _hookMeters.fontSharedMaterial = gui.interactNameText.fontSharedMaterial;
            _hookMeters.fontSize = HookMetersFontSize;
            _hookMeters.alignment = TextAlignmentOptions.Center;
            _hookMeters.textWrappingMode = TextWrappingModes.NoWrap;
            _hookMeters.raycastTarget = false;
        }

        private void CreateHookRetract(GUIManager gui, RectTransform parent)
        {
            _hookRetract = new GameObject(HookRetractName, typeof(RectTransform));
            RectTransform rect = (RectTransform)_hookRetract.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.left * HookIndicatorSize;
            rect.sizeDelta = new Vector2(HookIndicatorSize, HookIndicatorSize);
            Image arrow = _hookRetract.AddComponent<Image>();
            arrow.sprite = _retractSprite;
            arrow.material = gui.reticleDefaultImage.material;
            arrow.color = RepkaColors.HookLocked;
            arrow.preserveAspect = true;
            arrow.raycastTarget = false;
            _hookRetract.SetActive(false);
        }

        private void ShowHook(HookIndicatorState hook)
        {
            if (hook.Placed != _hookIndicator.activeSelf)
                _hookIndicator.SetActive(hook.Placed);

            if (!hook.Placed)
                return;

            Color color = hook.Locked ? RepkaColors.HookLocked : hook.AtMax ? RepkaColors.HookAtMax : _hookColor;
            _hookIcon.color = color;
            _hookMeters.color = color;
            int meters = Mathf.RoundToInt(hook.Meters);

            if (meters != _shownMeters)
            {
                _shownMeters = meters;
                _hookMeters.text = meters.ToString();
            }

            bool retracting = hook.Locked && hook.Retracting;

            if (retracting != _hookRetract.activeSelf)
                _hookRetract.SetActive(retracting);
        }

        private void CopyPulse(GameObject vanilla, GameObject target)
        {
            ScaleOnEnable pulse = vanilla.GetComponent<ScaleOnEnable>();

            if (pulse == null)
            {
                _log.LogWarning($"{nameof(CarabinerHud)}: vanilla reticle has no {nameof(ScaleOnEnable)}, carabiner reticle will not pulse");
                return;
            }

            ScaleOnEnable copy = target.AddComponent<ScaleOnEnable>();
            copy.time = pulse.time;
            FieldInfo ease = AccessTools.Field(typeof(ScaleOnEnable), PulseEaseField);
            ease.SetValue(copy, ease.GetValue(pulse));
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
            return ShowLine(prompt, line, new HudPromptLine(null, null), row);
        }

        private int ShowLine(HudPrompt prompt, HudPromptLine line, HudPromptLine side, int row)
        {
            if (line.Text == null)
            {
                prompt.Hide();
                return row;
            }

            prompt.Show(line, side, row);
            return row + 1;
        }
    }
}
