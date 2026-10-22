using System.Collections.Generic;
using BepInEx.Logging;
using Peak.UI;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class PlayerListBadges
    {
        private readonly LinkColors _colors;
        private readonly Sprite _sprite;
        private readonly ManualLogSource _log;
        private readonly Dictionary<AudioLevelSlider, LinkBadgeRow> _rows = new Dictionary<AudioLevelSlider, LinkBadgeRow>();
        private readonly HashSet<AudioLevelSlider> _warned = new HashSet<AudioLevelSlider>();
        private readonly List<LinkBadge> _buffer = new List<LinkBadge>();

        public PlayerListBadges(LinkColors colors, Sprite sprite, ManualLogSource log)
        {
            _colors = colors;
            _sprite = sprite;
            _log = log;
        }

        public void Forget()
        {
            _rows.Clear();
            _warned.Clear();
        }

        public void Show(AudioLevels levels, Material material)
        {
            foreach (AudioLevelSlider slider in levels.sliders)
            {
                if (slider.player == null || !slider.gameObject.activeInHierarchy)
                    continue;

                LinkBadgeRow row = RowOf(slider, material);
                _colors.CollectFor(slider.player.ActorNumber, _buffer);
                row.Show(_buffer);
                ReportOverlap(slider, row);
            }
        }

        private LinkBadgeRow RowOf(AudioLevelSlider slider, Material material)
        {
            if (!_rows.TryGetValue(slider, out LinkBadgeRow row))
            {
                KickButton kick = slider.GetComponentInChildren<KickButton>(true);
                RectTransform kickRect = kick == null ? null : (RectTransform)kick.transform;
                row = new LinkBadgeRow((RectTransform)slider.m_PlayerUI.playerAvatar.transform, slider.icon.rectTransform, kickRect, _sprite, material, slider.percent);
                _rows.Add(slider, row);
            }

            return row;
        }

        private void ReportOverlap(AudioLevelSlider slider, LinkBadgeRow row)
        {
            if (_warned.Contains(slider))
                return;

            bool hitsVolume = row.OverlapsVolume;
            bool hitsKick = row.OverlapsKick;

            if (!hitsVolume && !hitsKick)
                return;

            _warned.Add(slider);
            _log.LogWarning($"{nameof(PlayerListBadges)}: link badges of {slider.player.NickName} overlap volume {hitsVolume}, kick {hitsKick}");
        }
    }
}
