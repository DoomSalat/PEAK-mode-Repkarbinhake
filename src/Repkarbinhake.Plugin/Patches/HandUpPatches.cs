using HarmonyLib;
using Repkarbinhake.Plugin.Links;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class HandUpPatches // Static: Harmony patch class
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterGrabbing), nameof(CharacterGrabbing.TargetCanBeHelped), new[] { typeof(Character), typeof(Character) })]
        public static void HelpTiedTeammate(Character target, Character helper, ref bool __result)
        {
            if (__result || target == helper)
                return;

            if (helper.IsLocal && RepkaHooks.HandUpTarget != 0 && RepkaHooks.HandUpTarget == CharacterDirectory.ActorOf(target))
            {
                __result = true;
                return;
            }

            if (target.IsLocal && RepkaHooks.LocalHelpless && RepkaHooks.TiedPartners.Contains(CharacterDirectory.ActorOf(helper)))
                __result = true;
        }
    }
}
