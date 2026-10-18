using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
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
        private const string UnwindPromptName = "Repkarbinhake Unwind Prompt";
        private const string PartnerName = "Repkarbinhake Rope Partner";
        private const string PulseEaseField = "easeType";
        private const float ReticleSize = 48f;
        private const float PairGap = 40f;
        private const float PartnerGap = 16f;

        private readonly Sprite _sprite;
        private readonly HookIndicatorView _hookIndicator;
        private readonly ManualLogSource _log;
        private GameObject _reticle;
        private HudPrompt _prompt;
        private HudPrompt _pullPrompt;
        private HudPrompt _unwindPrompt;
        private GameObject _vanillaPrompt;
        private TMP_Text _ropeName;
        private Color _ropeNameColor;
        private TMP_Text _partner;

        public CarabinerHud(Sprite sprite, HookIndicatorView hookIndicator, ManualLogSource log)
        {
            _sprite = sprite;
            _hookIndicator = hookIndicator;
            _log = log;
        }

        public GameObject Reticle => _reticle;

        public void Attach(GUIManager gui)
        {
            CreateReticle(gui);
            _vanillaPrompt = gui.interactPromptPrimary;
            _prompt = new HudPrompt(_vanillaPrompt, PromptName, _log);
            _pullPrompt = new HudPrompt(_vanillaPrompt, PullPromptName, _log);
            _unwindPrompt = new HudPrompt(_vanillaPrompt, UnwindPromptName, _log);
            _unwindPrompt.LeaveLayout();
            CreatePartner(gui);
            _hookIndicator.Attach(gui);
        }

        public void Show(GUIManager gui, bool showReticle, HudPromptLine prompt, HudPromptLine pullPrompt, HudPromptLine unwindPrompt, string partner, Color partnerColor, HookIndicatorState hook)
        {
            if (_reticle == null || !_prompt.Exists || !_pullPrompt.Exists || !_unwindPrompt.Exists || _partner == null || !_hookIndicator.Exists)
                return;

            _hookIndicator.Show(hook);

            if (showReticle)
                gui.SetReticle(_reticle);

            _prompt.Show(prompt);
            ShowPair(pullPrompt, unwindPrompt);
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
            _ropeName = gui.interactNameText;
            _ropeNameColor = _ropeName.color;
            _partner = Object.Instantiate(_ropeName, _ropeName.transform.parent);
            _partner.name = PartnerName;

            foreach (LocalizedText localized in _partner.GetComponentsInChildren<LocalizedText>(true))
                Object.DestroyImmediate(localized);

            _partner.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _partner.raycastTarget = false;
            _partner.gameObject.SetActive(false);
        }

        private void ShowPair(HudPromptLine line, HudPromptLine side)
        {
            _pullPrompt.Show(line);

            if (line.Text == null || side.Text == null)
            {
                _unwindPrompt.Hide();
                return;
            }

            _unwindPrompt.Show(side);
            _unwindPrompt.PlaceRightOf(_pullPrompt, PairGap);
        }

        private void ShowPartner(string partner, Color color)
        {
            if (string.IsNullOrEmpty(partner) || !_vanillaPrompt.activeSelf || !_ropeName.gameObject.activeInHierarchy)
            {
                _partner.gameObject.SetActive(false);
                _ropeName.color = _ropeNameColor;
                return;
            }

            _ropeName.color = color;
            _partner.text = partner;
            _partner.color = color;
            _partner.gameObject.SetActive(true);
            HudPlacement.RightOf(_ropeName.rectTransform, _ropeName, _partner.rectTransform, _partner, PartnerGap);
        }
    }
}
