using HarmonyLib;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class RopeGaugePatches // Static: Harmony patch class
    {
        private const float MetersPerSegment = 0.25f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GUIManager), "UpdateRope")]
        public static bool ShowHeldRope(GUIManager __instance)
        {
            if (RepkaHooks.HeldRopeLength <= 0f || Character.localCharacter.data.currentItem != null)
                return true;

            __instance.ui_rope.gameObject.SetActive(true);
            __instance.ui_rope.UpdateRope(Mathf.RoundToInt(RepkaHooks.HeldRopeLength / MetersPerSegment));
            return false;
        }
    }
}
