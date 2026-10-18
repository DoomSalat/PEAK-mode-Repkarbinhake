using BepInEx.Configuration;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeRescue
    {
        private readonly ConfigEntry<bool> _enabled;

        public RopeRescue(ConfigEntry<bool> enabled)
        {
            _enabled = enabled;
        }

        public void Tick(Character local, bool pulledByTeammate)
        {
            if (!_enabled.Value || !pulledByTeammate)
                return;

            if (local.IsStuck())
                local.UnStick();

            CharacterAfflictions afflictions = local.refs.afflictions;
            Free(afflictions, CharacterAfflictions.STATUSTYPE.Web);
            Free(afflictions, CharacterAfflictions.STATUSTYPE.FlyTrap);
        }

        private void Free(CharacterAfflictions afflictions, CharacterAfflictions.STATUSTYPE status)
        {
            if (afflictions.GetCurrentStatus(status) > 0f)
                afflictions.SubtractStatus(status, 1f);
        }
    }
}
