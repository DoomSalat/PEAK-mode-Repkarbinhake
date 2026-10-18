using BepInEx.Configuration;
using Photon.Pun;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class HangRecovery
    {
        private const string UnFallRpc = "RPCA_UnFall";

        private readonly ConfigEntry<float> _hangSeconds;
        private readonly ConfigEntry<bool> _whenHeld;
        private float _hangingFor;

        public HangRecovery(ConfigEntry<float> hangSeconds, ConfigEntry<bool> whenHeld)
        {
            _hangSeconds = hangSeconds;
            _whenHeld = whenHeld;
        }

        public bool IsHanging { get; private set; }

        public void Tick(Character local, Footing footing, bool onTautRope, bool heldByTeammate, float deltaTime)
        {
            IsHanging = AutoHandUp.IsHelpless(footing) && onTautRope;

            if (footing != Footing.Airborne || !onTautRope)
            {
                _hangingFor = 0f;
                return;
            }

            _hangingFor += deltaTime;

            if (local.data.fallSeconds <= 0f)
                return;

            bool held = _whenHeld.Value && heldByTeammate;
            bool hungLongEnough = _hangSeconds.Value > 0f && _hangingFor >= _hangSeconds.Value;

            if (held || hungLongEnough)
                local.refs.view.RPC(UnFallRpc, RpcTarget.All);
        }
    }
}
