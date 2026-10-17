using HarmonyLib;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch(typeof(global::Interaction), nameof(global::Interaction.DoInteractableRaycasts))]
    public static class InteractionPatches // Static: Harmony patch class
    {
        private const float DefaultDistance = -1f;

        [HarmonyPostfix]
        public static void ApplyRopePriority(ref IInteractible interactableResult, float overrideDistance)
        {
            if (overrideDistance != DefaultDistance)
                return;

            if (RepkaHooks.HeldRope != null)
            {
                interactableResult = RepkaHooks.HeldRope;
                return;
            }

            if (RepkaHooks.AimedRope != null && interactableResult as Object == null)
                interactableResult = RepkaHooks.AimedRope;
        }
    }
}
