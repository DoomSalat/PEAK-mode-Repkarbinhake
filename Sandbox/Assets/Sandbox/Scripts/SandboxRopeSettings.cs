using Repkarbinhake.Core.Data;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    [CreateAssetMenu(fileName = nameof(SandboxRopeSettings), menuName = "Repkarbinhake/" + nameof(SandboxRopeSettings))]
    public sealed class SandboxRopeSettings : ScriptableObject, IRopeSettings, ISegmentSettings
    {
        [Header("Rope")]
        [SerializeField, Min(0.1f)] private float _minLength = 1.5f;
        [SerializeField, Min(0.5f)] private float _maxLength = 14f;
        [SerializeField, Min(0f)] private float _armStrength = 10f;
        [SerializeField, Min(0f)] private float _reelStaminaPerMeter = 0.1f;
        [SerializeField, Min(0f)] private float _reelSpeed = 6f;
        [SerializeField, Min(0f)] private float _stiffness = 30f;
        [SerializeField, Min(0f)] private float _damping = 6f;
        [SerializeField, Min(0f)] private float _maxPull = 35f;
        [SerializeField, Min(1f)] private float _snapDistance = 60f;
        [SerializeField, Min(0.005f)] private float _thickness = 0.045f;

        [Header("Stability")]
        [SerializeField, Range(0f, 1f)] private float _braceResistance = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _groupBonus = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _chainSupport = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _maxStability = 0.97f;

        [Header("Performance")]
        [SerializeField] private SegmentMode _segmentMode = SegmentMode.Auto;
        [SerializeField, Range(4, 64)] private int _fixedSegments = 24;
        [SerializeField, Range(4, 1024)] private int _segmentBudget = 240;
        [SerializeField, Range(4, 64)] private int _autoMaxSegments = 40;
        [SerializeField, Min(0.05f)] private float _segmentLength = 0.3f;

        [Header("Sandbox input")]
        [SerializeField, Min(0.5f)] private float _linkDistance = 6f;
        [SerializeField, Range(1f, 45f)] private float _linkAimAngle = 12f;
        [SerializeField, Min(0.01f)] private float _secondsPerScrollNotch = 0.08f;
        [SerializeField, Min(0.1f)] private float _tieHoldSeconds = 0.8f;
        [SerializeField, Min(0.1f)] private float _untieHoldSeconds = 1.2f;

        [Header("Hanging (proposal to test)")]
        [SerializeField] private bool _ropeSupportCapsAirTime = true;
        [SerializeField, Min(0f)] private float _hangAirTimeCap = 0.3f;
        [SerializeField, Min(0f)] private float _supportLoadThreshold = 3f;

        [Header("Cliff scenario (T)")]
        [SerializeField, Min(1.5f)] private float _cliffRopeLength = 5f;
        [SerializeField, Min(0f)] private float _pushSpeed = 5f;
        [SerializeField, Min(0f)] private float _pushUpward = 1.5f;
        [SerializeField, Min(0f)] private float _pushDelay = 0.75f;

        public float MinLength => _minLength;
        public float MaxLength => _maxLength;
        public float ArmStrength => _armStrength;
        public float ReelStaminaPerMeter => _reelStaminaPerMeter;
        public float ReelSpeed => _reelSpeed;
        public float Stiffness => _stiffness;
        public float Damping => _damping;
        public float MaxPull => _maxPull;
        public float SnapDistance => _snapDistance;
        public float Thickness => _thickness;
        public float BraceResistance => _braceResistance;
        public float GroupBonus => _groupBonus;
        public float ChainSupport => _chainSupport;
        public float MaxStability => _maxStability;
        public SegmentMode SegmentMode => _segmentMode;
        public int FixedSegments => _fixedSegments;
        public int SegmentBudget => _segmentBudget;
        public int AutoMaxSegments => _autoMaxSegments;
        public float SegmentLength => _segmentLength;
        public float LinkDistance => _linkDistance;
        public float LinkAimAngle => _linkAimAngle;
        public float SecondsPerScrollNotch => _secondsPerScrollNotch;
        public float TieHoldSeconds => _tieHoldSeconds;
        public float UntieHoldSeconds => _untieHoldSeconds;
        public bool RopeSupportCapsAirTime => _ropeSupportCapsAirTime;
        public float HangAirTimeCap => _hangAirTimeCap;
        public float SupportLoadThreshold => _supportLoadThreshold;
        public float CliffRopeLength => _cliffRopeLength;
        public float PushSpeed => _pushSpeed;
        public float PushUpward => _pushUpward;
        public float PushDelay => _pushDelay;
    }
}
