using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class HudPatches // Static: Harmony patch class
    {
        private const string ReticleName = "Repkarbinhake Carabiner Reticle";
        private const string PromptName = "Repkarbinhake Link Prompt";
        private const float ReticleSize = 48f;
        private const string KeyFallbackFormat = "[{0}]";

        private static GameObject _reticle; // Static: HUD object created by the GUIManager patch, read by the reticle patch
        private static GameObject _prompt; // Static: HUD object created by the GUIManager patch, read by the reticle patch
        private static TMP_Text _promptText; // Static: HUD object created by the GUIManager patch, read by the reticle patch
        private static TMP_Text _promptKey; // Static: HUD object created by the GUIManager patch, read by the reticle patch
        private static GameObject _vanillaPrompt; // Static: game prompt our prompt stacks above, read by the reticle patch
        private static Vector2 _promptHome; // Static: prompt position when the game prompt is hidden
        private static string _keyPath; // Static: input path the cached key icon was built for
        private static string _keyTag; // Static: cached key icon tag for the link key

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "Start")]
        public static void CreateWidgets(GUIManager __instance)
        {
            CreateCarabinerReticle(__instance);
            CreateLinkPrompt(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "UpdateReticle")]
        public static void UpdateWidgets(GUIManager __instance)
        {
            if (RepkaHooks.ShowCarabinerReticle && _reticle != null)
                __instance.SetReticle(_reticle);

            if (_prompt != null)
                ShowPrompt(RepkaHooks.LinkPrompt);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_UseItemProgress), "UpdateFillAmount")]
        public static void ShowLinkHoldProgress(UI_UseItemProgress __instance, ref bool __result)
        {
            if (__result || RepkaHooks.LinkHoldProgress <= 0f)
                return;

            __instance.fill.fillAmount = RepkaHooks.LinkHoldProgress;
            __result = true;
        }

        private static void CreateCarabinerReticle(GUIManager gui)
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
            icon.sprite = RepkaHooks.CarabinerSprite;
            icon.material = gui.reticleDefaultImage.material;
            icon.color = gui.reticleColorDefault;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            _reticle.SetActive(false);
            gui.reticleList.Add(_reticle);

            if (RepkaHooks.CarabinerSprite == null)
                RepkaHooks.Log.LogWarning($"{nameof(HudPatches)}: carabiner sprite is missing, reticle will be blank");
        }

        private static void CreateLinkPrompt(GUIManager gui)
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
                RepkaHooks.Log.LogWarning($"{nameof(HudPatches)}: interact prompt layout changed (key {_promptKey != null}, text {_promptText != null}), link prompt disabled");
        }

        private static void ShowPrompt(string text)
        {
            if (text == null || _promptKey == null || _promptText == null)
            {
                _prompt.SetActive(false);
                return;
            }

            RectTransform rect = (RectTransform)_prompt.transform;
            rect.anchoredPosition = _vanillaPrompt.activeSelf ? _promptHome + Vector2.up * rect.rect.height : _promptHome;
            _promptKey.spriteAsset = SingletonAsset<InputSpriteData>.Instance.keyboardSprites;
            _promptKey.text = KeyTag(RepkaHooks.LinkKeyPath);
            _promptText.text = text;
            _prompt.SetActive(true);
        }

        private static string KeyTag(string path)
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
