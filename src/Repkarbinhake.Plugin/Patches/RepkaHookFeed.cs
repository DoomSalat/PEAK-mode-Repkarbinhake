using Repkarbinhake.Core.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    public sealed class RepkaHookFeed
    {
        private readonly HudFeed _hud;
        private readonly HeldRopeFeed _heldRope;
        private readonly AssistFeed _assist;

        public RepkaHookFeed(HudFeed hud, HeldRopeFeed heldRope, AssistFeed assist)
        {
            _hud = hud;
            _heldRope = heldRope;
            _assist = assist;
        }

        public void Publish(int actor, Footing footing, Transform view, bool hanging)
        {
            _assist.Publish(actor, footing, hanging);
            _hud.Publish(actor);
            _heldRope.Publish(actor, footing, view);
        }

        public void Clear()
        {
            _assist.Clear();
            _hud.Clear();
            _heldRope.Clear();
        }
    }
}
