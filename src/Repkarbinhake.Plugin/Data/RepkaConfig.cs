using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class RepkaConfig : IRopeSettings, ISegmentSettings
    {
        private const string InputSection = "Input";
        private const string RopeSection = "Rope";
        private const string StabilitySection = "Stability";
        private const string HangingSection = "Hanging";
        private const string HookSection = "Hook";
        private const string KnockdownSection = "Knockdown";
        private const string VisualSection = "Visual";
        private const string PerformanceSection = "Performance";
        private const string DebugSection = "Debug";
        private const int SegmentLimit = 64;
        private const int BudgetLimit = 1024;
        private const string OrphanedEntriesProperty = "OrphanedEntries";
        private const string DebugWindowKey = "<Keyboard>/f7";

        private readonly ConfigEntry<float> _minLength;
        private readonly ConfigEntry<float> _maxLength;
        private readonly ConfigEntry<float> _reelSpeed;
        private readonly ConfigEntry<float> _armStrength;
        private readonly ConfigEntry<float> _reelStaminaPerMeter;
        private readonly ConfigEntry<float> _stiffness;
        private readonly ConfigEntry<float> _damping;
        private readonly ConfigEntry<float> _maxPull;
        private readonly ConfigEntry<float> _snapDistance;
        private readonly ConfigEntry<float> _braceResistance;
        private readonly ConfigEntry<float> _groupBonus;
        private readonly ConfigEntry<float> _chainSupport;
        private readonly ConfigEntry<float> _maxStability;
        private readonly ConfigEntry<float> _tieHoldSeconds;
        private readonly ConfigEntry<float> _untieHoldSeconds;
        private readonly ConfigEntry<SegmentMode> _segmentMode;
        private readonly ConfigEntry<int> _fixedSegments;
        private readonly ConfigEntry<int> _segmentBudget;
        private readonly ConfigEntry<int> _autoMaxSegments;
        private readonly ConfigEntry<float> _segmentLength;

        public RepkaConfig(ConfigFile file)
        {
            File = file;
            LinkKey = file.Bind(InputSection, "LinkKey", "<Keyboard>/g", "Look at a player and hold to tie or untie.");
            PullKey = file.Bind(InputSection, "PullKey", "<Keyboard>/g", "While holding a rope, hold to reel it in. Looking at an untied player, the link key ties instead.");
            UnwindKey = file.Bind(InputSection, "UnwindKey", "<Keyboard>/t", "While holding a rope, hold to let it out. If it is the same key as AnchorKey, it does not place or take back the wall hook while you hold a rope.");
            AnchorKey = file.Bind(InputSection, "AnchorKey", "<Keyboard>/t", "Hold to aim a wall hook, release to set it (tied to you). Press again to take it back. Ignored while holding a rope if UnwindKey is the same key.");
            LinkAimAngle = file.Bind(InputSection, "LinkAimAngle", 12f, "Max angle between view direction and the player (degrees).");
            RopeAimReach = file.Bind(InputSection, "RopeAimReach", 2.5f, "Max distance to the rope you grab with the interact key (meters).");
            RopeAimTolerance = file.Bind(InputSection, "RopeAimTolerance", 0.25f, "How close the view ray must pass the rope to aim at it (meters).");
            _tieHoldSeconds = file.Bind(InputSection, "TieHoldSeconds", 0.8f, "How long to hold LinkKey while looking at a player to tie.");
            _untieHoldSeconds = file.Bind(InputSection, "UntieHoldSeconds", 1.2f, "How long to hold LinkKey while looking at a tied player to untie.");

            _minLength = file.Bind(RopeSection, "MinLength", 0.1f, "Shortest rope length after reeling in (meters).");
            _maxLength = file.Bind(RopeSection, "MaxLength", 14f, "Full rope length for everyone in the lobby (meters). A loose rope is always this long, reeling shortens it only while held. Host value is used.");
            _reelSpeed = file.Bind(RopeSection, "ReelSpeed", 10f, "Reel speed in meters per second at full wheel speed (wheel speed is smoothed like the vanilla rope spool).");
            _armStrength = file.Bind(RopeSection, "ArmStrength", 90f, "Arm strength: reeling in slows down as rope load grows; at this load and above it crawls at the minimum speed. Teammates holding ropes to the same player add to it (first +60%, each next two thirds of the previous).");
            _reelStaminaPerMeter = file.Bind(RopeSection, "ReelStaminaPerMeter", 0.05f, "Stamina (full bar = 1) spent per meter reeled in at ArmStrength load. Slack rope is free, reeling out is free.");
            _stiffness = file.Bind(RopeSection, "Stiffness", 60f, "Pull acceleration per meter of overstretch.");
            _damping = file.Bind(RopeSection, "Damping", 6f, "Damping of the speed moving away from the partner.");
            _maxPull = file.Bind(RopeSection, "MaxPull", 80f, "Max pull acceleration from one rope.");
            _snapDistance = file.Bind(RopeSection, "SnapDistance", 60f, "Rope unties when partners are farther than this (teleports, respawns).");

            _braceResistance = file.Bind(StabilitySection, "BraceResistance", 0.8f, "Share of rope pull ignored while holding a rope on the ground (0..1).");
            _groupBonus = file.Bind(StabilitySection, "GroupBonus", 0.25f, "Each other bracing player in the same tied group removes this share of the remaining pull.");
            _chainSupport = file.Bind(StabilitySection, "ChainSupport", 0.6f, "How much the player holding your rope passes their own stability to you.");
            _maxStability = file.Bind(StabilitySection, "MaxStability", 0.97f, "Upper cap of stability, so a braced player can still be moved.");

            HangAirTimeCap = file.Bind(HangingSection, "HangAirTimeCap", 0.3f, "While a taut rope holds you up, PEAK air time (and its growing gravity) is capped to this many seconds.");
            SupportLoadThreshold = file.Bind(HangingSection, "SupportLoadThreshold", 3f, "Upward rope pull needed to count as hanging on the rope.");
            LedgePush = file.Bind(HangingSection, "LedgePush", 10f, "When a pulled rope holds you stuck under a ledge (in the air, barely moving for 0.3 s), the pull stops pressing you into the wall and pushes you off it this hard, so you swing around the ledge. 0 = off.");
            MantleStep = file.Bind(HangingSection, "MantleStep", 0.8f, "Help onto the edge: when a held rope hauls you up and you get stuck just under the edge, you are pushed onto a floor this far past the edge, toward the puller (meters).");
            MantlePush = file.Bind(HangingSection, "MantlePush", 30f, "Help onto the edge: how hard you are pushed onto that floor. 0 = off.");
            BelaySlideCap = file.Bind(HangingSection, "BelaySlideCap", 2f, "Belay: while a partner holds your rope, out-of-stamina wall slide time is capped to this (seconds, 0 = no slide build-up).");

            HookLength = file.Bind(HookSection, "HookLength", 14f, "Wall hook rope (T): longest rope from your hook to you (meters). Your own value, not synced with the host. Climbing farther than this, the hook rope holds you back.");
            AnchorStiffness = file.Bind(HookSection, "AnchorStiffness", 300f, "Wall hook rope (T): pull per meter of overstretch. Much stiffer than a rope between players, so a fall on the hook barely stretches it.");
            AnchorDamping = file.Bind(HookSection, "AnchorDamping", 20f, "Wall hook rope: damping of the speed moving away from the hook, stops bouncing after a fall.");
            HookRetractSpeed = file.Bind(HookSection, "HookRetractSpeed", 2f, "After the hook catches your fall, it reels you back up toward itself this fast until you stand or grab a wall (meters per second, 0 = off).");
            AnchorMaxPull = file.Bind(HookSection, "AnchorMaxPull", 250f, "Wall hook rope: max pull, high enough to stop a hard fall.");

            EnableKnockdown = file.Bind(KnockdownSection, "EnableKnockdown", true, "A very hard yank of the rope knocks you down (ragdoll) unless you hold that rope.");
            KnockdownSpeed = file.Bind(KnockdownSection, "KnockdownSpeed", 7f, "How fast the partner side must tighten the rope to knock you down (meters per second, about 0.7 s of free fall).");
            KnockdownSeconds = file.Bind(KnockdownSection, "KnockdownSeconds", 2f, "How long you stay knocked down (seconds).");

            Thickness = file.Bind(VisualSection, "Thickness", 0.045f, "Visual rope radius.");
            UseVanillaRopeMaterial = file.Bind(VisualSection, "UseVanillaRopeMaterial", true, "Use the game rope material. Turn off if the rope renders wrong.");
            RopeColor = file.Bind(VisualSection, "RopeColor", 0.85f, new ConfigDescription("How much the rope takes the hue of its color from the ESC player list (0 = plain brown rope, 1 = full color). Applies to ropes tied after the change.", new AcceptableValueRange<float>(0f, 1f)));
            SoundVolume = file.Bind(VisualSection, "SoundVolume", 0.6f, new ConfigDescription("Carabiner sound volume.", new AcceptableValueRange<float>(0f, 1f)));
            StableCamera = file.Bind(VisualSection, "StableCamera", false, "Motion sickness guard: while you are tied to a rope, the camera keeps looking where you aim instead of spinning with your head when you fall and dangle.");

            _segmentMode = file.Bind(PerformanceSection, "SegmentMode", SegmentMode.Auto, "Auto: bone budget is shared between all ropes, fewer ropes look smoother. Fixed: every rope gets at most FixedSegments bones.");
            _fixedSegments = file.Bind(PerformanceSection, "FixedSegments", 24, new ConfigDescription("Max bones per rope in Fixed mode.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, SegmentLimit)));
            _segmentBudget = file.Bind(PerformanceSection, "SegmentBudget", 240, new ConfigDescription("Total bones for all ropes in Auto mode.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, BudgetLimit)));
            _autoMaxSegments = file.Bind(PerformanceSection, "AutoMaxSegments", 40, new ConfigDescription("Max bones per rope in Auto mode, even with few ropes.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, SegmentLimit)));
            _segmentLength = file.Bind(PerformanceSection, "SegmentLength", 0.3f, "Preferred bone spacing (meters). Short ropes never get more bones than they need.");

            if (BuildFlavor.HasDebugTools)
            {
                ShowDebugOverlay = file.Bind(DebugSection, "ShowDebugOverlay", false, "Show rope numbers on screen.");
                TestDummyKey = file.Bind(DebugSection, "TestDummyKey", "<Keyboard>/f9", "Press to place a local test capsule where you look, then tie to it with the link key and pull it; press again to remove it.");
            }

            ConfigEditorKey = file.Bind(DebugSection, "ConfigEditorKey", BuildFlavor.HasDebugTools ? DebugWindowKey : string.Empty, "Key that opens the in-game settings window to edit and apply these settings while playing. Empty = no key; set it yourself, e.g. <Keyboard>/f7.");

            DropRemovedSettings(file);
        }

        public ConfigEntry<string> LinkKey { get; }
        public ConfigEntry<string> PullKey { get; }
        public ConfigEntry<string> UnwindKey { get; }
        public ConfigEntry<string> AnchorKey { get; }
        public ConfigEntry<float> LinkAimAngle { get; }
        public ConfigEntry<float> RopeAimReach { get; }
        public ConfigEntry<float> RopeAimTolerance { get; }
        public ConfigEntry<float> HangAirTimeCap { get; }
        public ConfigEntry<float> SupportLoadThreshold { get; }
        public ConfigEntry<float> LedgePush { get; }
        public ConfigEntry<float> MantleStep { get; }
        public ConfigEntry<float> MantlePush { get; }
        public ConfigEntry<float> HookLength { get; }
        public ConfigEntry<float> AnchorStiffness { get; }
        public ConfigEntry<float> AnchorDamping { get; }
        public ConfigEntry<float> AnchorMaxPull { get; }
        public ConfigEntry<float> HookRetractSpeed { get; }
        public ConfigEntry<float> BelaySlideCap { get; }
        public ConfigEntry<bool> EnableKnockdown { get; }
        public ConfigEntry<float> KnockdownSpeed { get; }
        public ConfigEntry<float> KnockdownSeconds { get; }
        public ConfigEntry<float> Thickness { get; }
        public ConfigEntry<bool> UseVanillaRopeMaterial { get; }
        public ConfigEntry<float> RopeColor { get; }
        public ConfigEntry<float> SoundVolume { get; }
        public ConfigEntry<bool> StableCamera { get; }
        public ConfigFile File { get; }
        public ConfigEntry<bool> ShowDebugOverlay { get; }
        public ConfigEntry<string> ConfigEditorKey { get; }
        public ConfigEntry<string> TestDummyKey { get; }
        public ConfigEntry<float> MaxLengthEntry => _maxLength;
        public float MinLength => _minLength.Value;
        public float MaxLength => _maxLength.Value;
        public float ReelSpeed => _reelSpeed.Value;
        public float ArmStrength => _armStrength.Value;
        public float ReelStaminaPerMeter => _reelStaminaPerMeter.Value;
        public float Stiffness => _stiffness.Value;
        public float Damping => _damping.Value;
        public float MaxPull => _maxPull.Value;
        public float SnapDistance => _snapDistance.Value;
        public float BraceResistance => _braceResistance.Value;
        public float GroupBonus => _groupBonus.Value;
        public float ChainSupport => _chainSupport.Value;
        public float MaxStability => _maxStability.Value;
        public float TieHoldSeconds => _tieHoldSeconds.Value;
        public float UntieHoldSeconds => _untieHoldSeconds.Value;
        public SegmentMode SegmentMode => _segmentMode.Value;
        public int FixedSegments => _fixedSegments.Value;
        public int SegmentBudget => _segmentBudget.Value;
        public int AutoMaxSegments => _autoMaxSegments.Value;
        public float SegmentLength => _segmentLength.Value;

        private void DropRemovedSettings(ConfigFile file)
        {
            Dictionary<ConfigDefinition, string> orphaned = (Dictionary<ConfigDefinition, string>)AccessTools.Property(typeof(ConfigFile), OrphanedEntriesProperty).GetValue(file);

            if (orphaned.Count == 0)
                return;

            orphaned.Clear();
            file.Save();
        }
    }
}
