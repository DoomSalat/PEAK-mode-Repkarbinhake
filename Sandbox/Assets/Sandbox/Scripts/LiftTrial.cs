using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class LiftTrial
    {
        private const float EdgeReachDepth = 1.7f;
        private const float NotStarted = -1f;
        private const float Percent = 100f;

        private SandboxClimber _holder;
        private SandboxClimber _hanger;
        private Vector3 _holderStart;
        private float _ledgeHeight;
        private float _elapsed;
        private float _liftStartedAt = NotStarted;
        private float _depthAtLiftStart;
        private float _liftDuration = NotStarted;

        public bool IsRunning => _holder != null;
        public bool IsLifting => _liftStartedAt >= 0f;
        public bool IsLifted => _liftDuration >= 0f;
        public float Elapsed => _elapsed;
        public float PeakLoad { get; private set; }
        public float PeakHolderPull { get; private set; }
        public float HolderSlide { get; private set; }
        public float MaxDepth { get; private set; }
        public float CurrentDepth { get; private set; }
        public float HolderStaminaSpent { get; private set; }

        public void Begin(SandboxClimber holder, SandboxClimber hanger, float ledgeHeight)
        {
            _holder = holder;
            _hanger = hanger;
            _holderStart = holder.Center;
            _ledgeHeight = ledgeHeight;
            _elapsed = 0f;
            _liftStartedAt = NotStarted;
            _depthAtLiftStart = 0f;
            _liftDuration = NotStarted;
            PeakLoad = 0f;
            PeakHolderPull = 0f;
            HolderSlide = 0f;
            MaxDepth = 0f;
            CurrentDepth = 0f;
            HolderStaminaSpent = 0f;
        }

        public void MarkLiftStart()
        {
            if (!IsRunning || IsLifting)
                return;

            _liftStartedAt = _elapsed;
            _depthAtLiftStart = CurrentDepth;
        }

        public void AddStaminaSpent(int actor, float amount)
        {
            if (IsRunning && actor == _holder.Actor)
                HolderStaminaSpent += amount;
        }

        public void Stop()
        {
            _holder = null;
            _hanger = null;
        }

        public void Tick(float deltaTime, float load, float holderPull)
        {
            if (!IsRunning)
                return;

            _elapsed += deltaTime;
            PeakLoad = Mathf.Max(PeakLoad, load);
            PeakHolderPull = Mathf.Max(PeakHolderPull, holderPull);
            HolderSlide = Mathf.Max(HolderSlide, Vector3.ProjectOnPlane(_holder.Center - _holderStart, Vector3.up).magnitude);
            CurrentDepth = Mathf.Max(_ledgeHeight - _hanger.Center.y, 0f);
            MaxDepth = Mathf.Max(MaxDepth, CurrentDepth);

            if (IsLifting && !IsLifted && _depthAtLiftStart > EdgeReachDepth && CurrentDepth <= EdgeReachDepth)
                _liftDuration = _elapsed - _liftStartedAt;
        }

        public string Report()
        {
            string outcome = IsLifted ? $"reached edge after {_liftDuration:0.0}s of reeling" : $"NOT at edge after {_elapsed:0.0}s";
            return $"{nameof(LiftTrial)}: {outcome}; depthAtStart={_depthAtLiftStart:0.0}m; depthNow={CurrentDepth:0.0}m; maxDepth={MaxDepth:0.0}m; peakLoad={PeakLoad:0.0}; peakHolderPull={PeakHolderPull:0.0}; holderSlide={HolderSlide:0.00}m; holderStaminaSpent={HolderStaminaSpent * Percent:0}%; holderStaminaLeft={_holder.Stamina * Percent:0}%";
        }

        public void Describe(List<string> lines)
        {
            if (!IsRunning)
                return;

            string lifted = IsLifted ? $"дотянут до края за {_liftDuration:0.0} с смотки" : $"висит {_elapsed:0.0} с";
            lines.Add($"СЦЕНА СКАЛА: пиковая нагрузка {PeakLoad:0.0}, пиковая тяга на держащего {PeakHolderPull:0.0}, держащего протащило {HolderSlide:0.00} м, стамины держащего потрачено {HolderStaminaSpent * Percent:0}%");
            lines.Add($"   глубина: сейчас {CurrentDepth:0.0} м, максимум {MaxDepth:0.0} м, {lifted}, гравитация висящего {_hanger.CurrentGravity:0.0}");
        }
    }
}
