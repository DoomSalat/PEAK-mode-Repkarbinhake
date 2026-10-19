using BepInEx.Configuration;
using Photon.Pun;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class HangRecovery
    {
        private const string UnFallRpc = "RPCA_UnFall";

        private readonly ConfigEntry<bool> _whenHeld;

        public HangRecovery(ConfigEntry<bool> whenHeld)
        {
            _whenHeld = whenHeld;
        }

        public bool IsHanging { get; private set; }

        public void Tick(Character local, Footing footing, bool onTautRope, bool heldByTeammate)
        {
            IsHanging = AutoHandUp.IsHelpless(footing) && onTautRope;

            if (!_whenHeld.Value || footing != Footing.Airborne || !heldByTeammate || local.data.fallSeconds <= 0f)
                return;

            local.refs.view.RPC(UnFallRpc, RpcTarget.All);
            local.data.currentRagdollControll = local.data.GetTargetRagdollControll();
        }
    }
}
