using HarmonyLib;
using Repkarbinhake.Plugin.Handling;
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
            RepkaHooks.Hud?.Show(__instance, RepkaHooks.ShowCarabinerReticle, RepkaHooks.LinkPrompt, RepkaHooks.LinkKeyPath, aimsRope ? rope.PartnerName : null, aimsRope ? rope.Color : Color.white);
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
