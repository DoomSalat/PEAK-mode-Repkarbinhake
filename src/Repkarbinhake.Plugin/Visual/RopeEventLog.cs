using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using Photon.Pun;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Patches;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeEventLog
    {
        private const string Separator = ", ";
        private const string ColorEnd = "</color>";

        private readonly ManualLogSource _log;
        private readonly Dictionary<int, List<int>> _untied = new Dictionary<int, List<int>>();
        private readonly StringBuilder _names = new StringBuilder();
        private readonly StringBuilder _plainNames = new StringBuilder();

        public RopeEventLog(ManualLogSource log)
        {
            _log = log;
        }

        public void Untied(int actor, int partner)
        {
            if (!_untied.TryGetValue(actor, out List<int> partners))
            {
                partners = new List<int>();
                _untied[actor] = partners;
            }

            if (!partners.Contains(partner))
                partners.Add(partner);
        }

        public void TiesChanged(bool locked)
        {
            Announce(!locked, locked ? RepkaTexts.TiesForbidden : RepkaTexts.TiesAllowed, locked ? "tying and untying disabled" : "tying and untying allowed");
        }

        public void HooksChanged(bool allowed)
        {
            Announce(allowed, allowed ? RepkaTexts.HooksAllowed : RepkaTexts.HooksForbidden, allowed ? "safety hooks allowed" : "safety hooks disabled");
        }

        public void Flush()
        {
            foreach (KeyValuePair<int, List<int>> pair in _untied)
            {
                if (pair.Value.Count > 0)
                    Post(pair.Key, pair.Value);

                pair.Value.Clear();
            }
        }

        public void Clear()
        {
            foreach (List<int> partners in _untied.Values)
                partners.Clear();
        }

        private void Announce(bool allowed, string text, string english)
        {
            _log.LogInfo($"{nameof(RopeEventLog)}: host: {english}");
            PlayerConnectionLog feed = RepkaHooks.ConnectionLog;

            if (feed != null)
                feed.AddMessage($"{feed.GetColorTag(allowed ? feed.joinedColor : feed.leftColor)}[{RepkaPlugin.Name}] {text}{ColorEnd}");
        }

        private void Post(int actor, List<int> partners)
        {
            PlayerConnectionLog feed = RepkaHooks.ConnectionLog;
            string nameColor = feed != null ? feed.GetColorTag(feed.userColor) : string.Empty;
            _names.Clear();
            _plainNames.Clear();

            foreach (int partner in partners)
            {
                if (_plainNames.Length > 0)
                {
                    _names.Append(Separator);
                    _plainNames.Append(Separator);
                }

                string name = NameOf(partner);
                _names.Append(nameColor).Append(name).Append(ColorEnd);
                _plainNames.Append(name);
            }

            _log.LogInfo($"{nameof(RopeEventLog)}: {NameOf(actor)} untied from {_plainNames}");

            if (feed == null)
                return;

            string text = string.Format(RepkaTexts.UntiedFrom(partners.Count), nameColor + NameOf(actor) + ColorEnd, _names);
            feed.AddMessage($"{feed.GetColorTag(feed.leftColor)}[{RepkaPlugin.Name}] {text}{ColorEnd}");
        }

        private string NameOf(int actor)
        {
            return PhotonNetwork.CurrentRoom?.GetPlayer(actor)?.NickName ?? actor.ToString();
        }
    }
}
