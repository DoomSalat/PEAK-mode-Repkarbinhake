using HarmonyLib;
using Repkarbinhake.Plugin.Handling;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class HoldPosePatches // Static: Harmony patch class
    {
        private const string IdleParameter = "Idle";

        private static readonly int IdleHash = Animator.StringToHash(IdleParameter); // Static: animator hash computed once for all characters

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterAnimations), "Update")]
        public static void HoldSpoolIdle(CharacterAnimations __instance, Character ___character)
        {
            if (TryGetSpool(___character, out _))
                ___character.refs.animator.SetFloat(IdleHash, __instance.holdingItemIdleIndex);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterAnimations), "HandleIK")]
        public static void EnableSpoolIK(Character ___character)
        {
            if (___character.refs.ikRight == null || !TryGetSpool(___character, out _))
                return;

            ___character.refs.ikRig.weight = 1f;
            ___character.refs.ikRight.weight = 1f;
            ___character.refs.ikLeft.weight = 1f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterRagdoll), "SaveAdditionalTransformPositions")]
        public static void FollowSpoolHoldPoint(Character ___character)
        {
            if (!TryGetSpool(___character, out Item spool))
                return;

            CharacterData data = ___character.data;
            Vector3 forward = data.lookDirection * spool.defaultForward.z + data.lookDirection_Right * spool.defaultForward.x + data.lookDirection_Up * spool.defaultForward.y;
            Transform holdPoint = ___character.refs.animationItemTransform;
            holdPoint.SetPositionAndRotation(___character.refs.animationLookTransform.TransformPoint(spool.defaultPos), Quaternion.LookRotation(forward));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterAnimations), nameof(CharacterAnimations.ConfigureIK))]
        public static void PlaceSpoolHands(Character ___character)
        {
            if (___character.refs.IKHandTargetLeft == null || !TryGetSpool(___character, out Item spool))
                return;

            Character.CharacterRefs refs = ___character.refs;
            refs.IKHandTargetRight.SetPositionAndRotation(refs.items.GetItemPosRight(spool), refs.items.GetItemRotRight(spool));
            refs.IKHandTargetLeft.SetPositionAndRotation(refs.items.GetItemPosLeft(spool), refs.items.GetItemRotLeft(spool));
        }

        private static bool TryGetSpool(Character character, out Item spool)
        {
            spool = RepkaHooks.Spool == null ? null : RepkaHooks.Spool.Item;
            return spool != null && RepkaHooks.HoldPoses != null && character.data.currentItem == null && RepkaHooks.HoldPoses.ContainsKey(character);
        }
    }
}
