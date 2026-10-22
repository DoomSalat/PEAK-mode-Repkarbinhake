using System.Collections.Generic;

namespace Repkarbinhake.Plugin.Data
{
    public static class SettingScopes // Static: fixed table of who each config entry affects, shared by the mod UI
    {
        private const SettingScope OnYou = SettingScope.OnYou;
        private const SettingScope WhenPulling = SettingScope.WhenPulling;
        private const SettingScope OnYouWhenPulling = SettingScope.OnYouWhenPulling;
        private const SettingScope HostToAll = SettingScope.HostToAll;

        private static readonly Dictionary<string, SettingScope> Scopes = new Dictionary<string, SettingScope> // Static: fixed lookup table
        {
            { "Stiffness", OnYou },
            { "Damping", OnYou },
            { "MaxPull", OnYou },
            { "HaulStiffness", OnYou },
            { "SnapDistance", OnYou },
            { "BraceResistance", OnYou },
            { "GroupBonus", OnYou },
            { "ChainSupport", OnYou },
            { "MaxStability", OnYou },
            { "HangAirTimeCap", OnYou },
            { "SupportLoadThreshold", OnYou },
            { "BelaySlideCap", OnYou },
            { "LedgePush", OnYou },
            { "ClimbHaulForce", OnYou },
            { "MantleStep", OnYou },
            { "MantlePush", OnYou },
            { "FreeWhenPulled", OnYou },
            { "RecoverWhenHeld", OnYou },
            { "HookLength", OnYou },
            { "AnchorStiffness", OnYou },
            { "AnchorDamping", OnYou },
            { "AnchorMaxPull", OnYou },
            { "HookRetractSpeed", OnYou },
            { "EnableKnockdown", OnYou },
            { "KnockdownSpeed", OnYou },
            { "KnockdownSeconds", OnYou },
            { "AutoHandUp", WhenPulling },
            { "ReelSpeed", WhenPulling },
            { "ArmStrength", WhenPulling },
            { "ReelStaminaPerMeter", WhenPulling },
            { "MinLength", OnYouWhenPulling },
            { "MaxLength", HostToAll },
            { "AllowHooks", HostToAll },
            { "LockLinks", HostToAll },
            { "AllowHooksKey", HostToAll },
            { "LockLinksKey", HostToAll },
        };

        public static string Of(string key)
        {
            return RepkaTexts.ScopeName(Scopes.TryGetValue(key, out SettingScope scope) ? scope : SettingScope.LocalOnly);
        }
    }
}
