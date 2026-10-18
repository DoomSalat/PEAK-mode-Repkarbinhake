using HarmonyLib;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class CameraPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainCameraMovement), "CharacterCam")]
        public static void KeepLevelWhileTied(MainCameraMovement __instance)
        {
            Character local = Character.localCharacter;

            if (!RepkaHooks.StableCamera || local == null || local.data.lookDirection == Vector3.zero)
                return;

            Transform camera = __instance.transform;
            camera.rotation = Quaternion.LookRotation(local.data.lookDirection);
            camera.Rotate(GamefeelHandler.instance.GetRotation(), Space.World);
        }
    }
}
