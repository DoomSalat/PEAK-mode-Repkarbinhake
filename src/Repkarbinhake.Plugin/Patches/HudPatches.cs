using HarmonyLib;

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
            RepkaHooks.Hud?.Show(__instance, RepkaHooks.ShowCarabinerReticle, RepkaHooks.LinkPrompt, RepkaHooks.LinkKeyPath);
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
    }
}
