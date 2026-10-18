using HarmonyLib;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class HudPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "Start")]
        public static void CreateWidgets(GUIManager __instance)
        {
            RepkaHooks.Hud?.Attach(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "UpdateReticle")]
        public static void UpdateWidgets(GUIManager __instance)
        {
            RopeInteractible rope = __instance.currentInteractable as RopeInteractible;
            bool aimsRope = rope != null;
            if (aimsRope && __instance.interactPromptText.text != rope.GetInteractionText())
                __instance.RefreshInteractablePrompt();

            bool vanillaShowsRope = aimsRope && __instance.interactPromptPrimary.activeSelf;
            HudPromptLine prompt = new HudPromptLine(RepkaHooks.ReleasePrompt && vanillaShowsRope ? null : RepkaHooks.LinkPrompt, RepkaHooks.LinkKeyPath);
            HudPromptLine pullPrompt = new HudPromptLine(RepkaHooks.PullPrompt, RepkaHooks.PullKeyPath);
            HudPromptLine unwindPrompt = new HudPromptLine(RepkaHooks.UnwindPrompt, RepkaHooks.UnwindKeyPath);
            RepkaHooks.Hud?.Show(__instance, RepkaHooks.ShowCarabinerReticle, prompt, pullPrompt, unwindPrompt, aimsRope ? rope.PartnerName : null, aimsRope ? rope.Color : Color.white, new HookIndicatorState(RepkaHooks.HookPlaced, RepkaHooks.HookLocked, RepkaHooks.HookAtMax, RepkaHooks.HookRetracting, RepkaHooks.HookMeters));
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "UpdateWindowStatus")]
        public static void BlockInputForConfigWindow(GUIManager __instance)
        {
            if (!RepkaHooks.ConfigWindowOpen)
                return;

            __instance.windowShowingCursor = true;
            __instance.windowBlockingInput = true;
        }
    }
}
