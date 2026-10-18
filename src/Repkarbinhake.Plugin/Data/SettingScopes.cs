using System.Collections.Generic;

namespace Repkarbinhake.Plugin.Data
{
    public static class SettingScopes // Static: fixed table of who each config entry affects, shared by the mod UI
    {
        public const string OnYou = "на тебя";
        public const string WhenPulling = "когда тянешь";
        public const string OnYouWhenPulling = "на тебя и когда тянешь";
        public const string HostToAll = "хост → всем";
        public const string LocalOnly = "только у тебя";

        private static readonly Dictionary<string, string> Scopes = new Dictionary<string, string> // Static: fixed lookup table
        {
            { "Stiffness", OnYou },
            { "Damping", OnYou },
            { "MaxPull", OnYou },
            { "SnapDistance", OnYou },
            { "BraceResistance", OnYou },
            { "GroupBonus", OnYou },
            { "ChainSupport", OnYou },
            { "MaxStability", OnYou },
            { "HangAirTimeCap", OnYou },
            { "SupportLoadThreshold", OnYou },
            { "BelaySlideCap", OnYou },
            { "LedgePush", OnYou },
            { "MantleStep", OnYou },
            { "MantlePush", OnYou },
            { "FreeWhenPulled", OnYou },
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
        };

        public static string Of(string key)
        {
            return Scopes.TryGetValue(key, out string scope) ? scope : LocalOnly;
        }
    }
}
