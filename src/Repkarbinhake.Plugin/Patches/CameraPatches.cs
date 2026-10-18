using HarmonyLib;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class CameraPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainCameraMovement), "CharacterCam")]
        public static void SteadyWhileTied(MainCameraMovement __instance)
        {
            Character local = Character.localCharacter;

            if (local == null || RepkaHooks.HangingCamera == null)
                return;

            RepkaHooks.HangingCamera.Apply(__instance.transform, local.data.lookDirection, RepkaHooks.StableCamera, RepkaHooks.CameraHanging, Time.deltaTime);
        }
    }
}
