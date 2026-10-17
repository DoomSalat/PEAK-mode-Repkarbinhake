using BepInEx.Configuration;
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
        private const string VisualSection = "Visual";
        private const string PerformanceSection = "Performance";
        private const string DebugSection = "Debug";
        private const int SegmentLimit = 64;
        private const int BudgetLimit = 1024;

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
            LinkKey = file.Bind(InputSection, "LinkKey", "<Keyboard>/g", "Look at a player and hold to tie or untie.");
            LinkAimAngle = file.Bind(InputSection, "LinkAimAngle", 12f, "Max angle between view direction and the player (degrees).");
            RopeAimReach = file.Bind(InputSection, "RopeAimReach", 2.5f, "Max distance to the rope you grab with the interact key (meters).");
            RopeAimTolerance = file.Bind(InputSection, "RopeAimTolerance", 0.25f, "How close the view ray must pass the rope to aim at it (meters).");
            _tieHoldSeconds = file.Bind(InputSection, "TieHoldSeconds", 0.8f, "How long to hold LinkKey while looking at a player to tie.");
            _untieHoldSeconds = file.Bind(InputSection, "UntieHoldSeconds", 1.2f, "How long to hold LinkKey while looking at a tied player to untie.");

            _minLength = file.Bind(RopeSection, "MinLength", 1.5f, "Shortest rope length after reeling in (meters).");
            _maxLength = file.Bind(RopeSection, "MaxLength", 14f, "Full rope length for everyone in the lobby (meters). A loose rope is always this long, reeling shortens it only while held. Host value is used.");
            _reelSpeed = file.Bind(RopeSection, "ReelSpeed", 6f, "Reel speed in meters per second at full wheel speed (wheel speed is smoothed like the vanilla rope spool).");
            _armStrength = file.Bind(RopeSection, "ArmStrength", 12f, "Arm strength: reeling in slows down as rope load grows; at this load and above it crawls at the minimum speed. Only slightly above a hanging player weight, so lifting someone is really hard.");
            _reelStaminaPerMeter = file.Bind(RopeSection, "ReelStaminaPerMeter", 0.1f, "Stamina (full bar = 1) spent per meter reeled in at ArmStrength load. Slack rope is free, reeling out is free.");
            _stiffness = file.Bind(RopeSection, "Stiffness", 30f, "Pull acceleration per meter of overstretch.");
            _damping = file.Bind(RopeSection, "Damping", 6f, "Damping of the speed moving away from the partner.");
            _maxPull = file.Bind(RopeSection, "MaxPull", 35f, "Max pull acceleration from one rope.");
            _snapDistance = file.Bind(RopeSection, "SnapDistance", 60f, "Rope unties when partners are farther than this (teleports, respawns).");

            _braceResistance = file.Bind(StabilitySection, "BraceResistance", 0.8f, "Share of rope pull ignored while holding a rope on the ground (0..1).");
            _groupBonus = file.Bind(StabilitySection, "GroupBonus", 0.25f, "Each other bracing player in the same tied group removes this share of the remaining pull.");
            _chainSupport = file.Bind(StabilitySection, "ChainSupport", 0.6f, "How much the player holding your rope passes their own stability to you.");
            _maxStability = file.Bind(StabilitySection, "MaxStability", 0.97f, "Upper cap of stability, so a braced player can still be moved.");

            HangAirTimeCap = file.Bind(HangingSection, "HangAirTimeCap", 0.3f, "While a taut rope holds you up, PEAK air time (and its growing gravity) is capped to this many seconds.");
            SupportLoadThreshold = file.Bind(HangingSection, "SupportLoadThreshold", 3f, "Upward rope pull needed to count as hanging on the rope.");
            BelaySlideCap = file.Bind(HangingSection, "BelaySlideCap", 2f, "Belay: while a partner holds your rope, out-of-stamina wall slide time is capped to this (seconds, 0 = no slide build-up).");

            Thickness = file.Bind(VisualSection, "Thickness", 0.045f, "Visual rope radius.");
            UseVanillaRopeMaterial = file.Bind(VisualSection, "UseVanillaRopeMaterial", true, "Use the game rope material. Turn off if the rope renders wrong.");
            SoundVolume = file.Bind(VisualSection, "SoundVolume", 0.6f, new ConfigDescription("Carabiner sound volume.", new AcceptableValueRange<float>(0f, 1f)));

            _segmentMode = file.Bind(PerformanceSection, "SegmentMode", SegmentMode.Auto, "Auto: bone budget is shared between all ropes, fewer ropes look smoother. Fixed: every rope gets at most FixedSegments bones.");
            _fixedSegments = file.Bind(PerformanceSection, "FixedSegments", 24, new ConfigDescription("Max bones per rope in Fixed mode.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, SegmentLimit)));
            _segmentBudget = file.Bind(PerformanceSection, "SegmentBudget", 240, new ConfigDescription("Total bones for all ropes in Auto mode.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, BudgetLimit)));
            _autoMaxSegments = file.Bind(PerformanceSection, "AutoMaxSegments", 40, new ConfigDescription("Max bones per rope in Auto mode, even with few ropes.", new AcceptableValueRange<int>(RopeSegmentation.MinSegments, SegmentLimit)));
            _segmentLength = file.Bind(PerformanceSection, "SegmentLength", 0.3f, "Preferred bone spacing (meters). Short ropes never get more bones than they need.");

            EnableTestAnchor = file.Bind(DebugSection, "EnableTestAnchor", true, "Allow tying yourself to a test anchor (local only, for solo testing).");
            TestAnchorKey = file.Bind(DebugSection, "TestAnchorKey", "<Keyboard>/f8", "Press to place a test anchor where you look and tie to it; press again to remove it.");
            EnableTestDummy = file.Bind(DebugSection, "EnableTestDummy", true, "Allow spawning a local test dummy to tie to and drag (solo testing).");
            TestDummyKey = file.Bind(DebugSection, "TestDummyKey", "<Keyboard>/f9", "Press to place a test dummy where you look, then tie to it with the link key; press again to remove it.");
            ShowDebugOverlay = file.Bind(DebugSection, "ShowDebugOverlay", false, "Show rope numbers on screen.");
        }

        public ConfigEntry<string> LinkKey { get; }
        public ConfigEntry<float> LinkAimAngle { get; }
        public ConfigEntry<float> RopeAimReach { get; }
        public ConfigEntry<float> RopeAimTolerance { get; }
        public ConfigEntry<float> HangAirTimeCap { get; }
        public ConfigEntry<float> SupportLoadThreshold { get; }
        public ConfigEntry<float> BelaySlideCap { get; }
        public ConfigEntry<float> Thickness { get; }
        public ConfigEntry<bool> UseVanillaRopeMaterial { get; }
        public ConfigEntry<float> SoundVolume { get; }
        public ConfigEntry<bool> EnableTestAnchor { get; }
        public ConfigEntry<string> TestAnchorKey { get; }
        public ConfigEntry<bool> EnableTestDummy { get; }
        public ConfigEntry<string> TestDummyKey { get; }
        public ConfigEntry<bool> ShowDebugOverlay { get; }
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
    }
}
