using HarmonyLib;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class PlayerListPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "Start")]
        public static void ForgetRows()
        {
            RepkaHooks.Badges?.Forget();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AudioLevels), "Update")]
        public static void ShowLinkBadges(AudioLevels __instance)
        {
            if (GUIManager.instance != null)
                RepkaHooks.Badges?.Show(__instance, GUIManager.instance.reticleDefaultImage.material);
        }
    }
}
