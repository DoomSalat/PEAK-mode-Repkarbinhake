using HarmonyLib;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class ConnectionLogPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerConnectionLog), "Awake")]
        public static void Remember(PlayerConnectionLog __instance)
        {
            RepkaHooks.ConnectionLog = __instance;
        }
    }
}
