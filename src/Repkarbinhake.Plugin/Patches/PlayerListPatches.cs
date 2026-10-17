using System.Collections.Generic;
using HarmonyLib;
using Peak.UI;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    [HarmonyPatch]
    public static class PlayerListPatches // Static: Harmony patch class
    {
        private static readonly Dictionary<AudioLevelSlider, LinkBadgeRow> Rows = new Dictionary<AudioLevelSlider, LinkBadgeRow>(); // Static: badge rows of the current pause menu
        private static readonly HashSet<AudioLevelSlider> Warned = new HashSet<AudioLevelSlider>(); // Static: rows already reported as overlapping
        private static readonly List<Color> Colors = new List<Color>(); // Static: reused buffer for badge colors

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GUIManager), "Start")]
        public static void ForgetRows()
        {
            Rows.Clear();
            Warned.Clear();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AudioLevels), "Update")]
        public static void ShowLinkBadges(AudioLevels __instance)
        {
            if (RepkaHooks.LinkColors == null || GUIManager.instance == null)
                return;

            foreach (AudioLevelSlider slider in __instance.sliders)
            {
                if (slider.player == null || !slider.gameObject.activeInHierarchy)
                    continue;

                LinkBadgeRow row = RowOf(slider);
                RepkaHooks.LinkColors.CollectFor(slider.player.ActorNumber, Colors);
                row.Show(Colors);
                ReportOverlap(slider, row);
            }
        }

        private static LinkBadgeRow RowOf(AudioLevelSlider slider)
        {
            if (!Rows.TryGetValue(slider, out LinkBadgeRow row))
            {
                KickButton kick = slider.GetComponentInChildren<KickButton>(true);
                RectTransform kickRect = kick == null ? null : (RectTransform)kick.transform;
                row = new LinkBadgeRow((RectTransform)slider.m_PlayerUI.playerAvatar.transform, slider.icon.rectTransform, kickRect, RepkaHooks.CarabinerSprite, GUIManager.instance.reticleDefaultImage.material);
                Rows.Add(slider, row);
            }

            return row;
        }

        private static void ReportOverlap(AudioLevelSlider slider, LinkBadgeRow row)
        {
            if (Warned.Contains(slider))
                return;

            bool hitsVolume = row.OverlapsVolume;
            bool hitsKick = row.OverlapsKick;

            if (!hitsVolume && !hitsKick)
                return;

            Warned.Add(slider);
            RepkaHooks.Log.LogWarning($"{nameof(PlayerListPatches)}: link badges of {slider.player.NickName} overlap volume {hitsVolume}, kick {hitsKick}");
        }
    }
}
