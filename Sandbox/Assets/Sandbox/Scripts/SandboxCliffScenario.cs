using System;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxCliffScenario
    {
        private const int HolderActor = 1;
        private const int HangerActor = 2;
        private const float FallSeconds = 2.5f;
        private const float StandHeight = 1.05f;
        private const float AutoLiftSettleSeconds = 1.5f;
        private const float AutoLiftTimeout = 45f;
        private const float AutoLiftLogInterval = 5f;

        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly SandboxRopeForces _forces;
        private readonly SandboxRopeSettings _settings;
        private readonly SandboxCrew _crew;
        private readonly Transform _holderSpot;
        private readonly Transform _edgeSpot;
        private readonly Action<float, float> _reelSelected;
        private SandboxClimber _pushTarget;
        private float _pushTimer;
        private bool _autoLift;
        private float _autoLiftWait;
        private float _autoLiftLogTimer;

        public SandboxCliffScenario(LinkRegistry registry, RopeReel reel, SandboxRopeForces forces, SandboxRopeSettings settings, SandboxCrew crew, Transform holderSpot, Transform edgeSpot, Action<float, float> reelSelected)
        {
            _registry = registry;
            _reel = reel;
            _forces = forces;
            _settings = settings;
            _crew = crew;
            _holderSpot = holderSpot;
            _edgeSpot = edgeSpot;
            _reelSelected = reelSelected;
        }

        public LiftTrial Trial { get; } = new LiftTrial();
        public SandboxClimber Holder => _crew.ClimberOf(HolderActor);

        public void Push(SandboxClimber target, Vector3 from)
        {
            Vector3 direction = Vector3.ProjectOnPlane(target.Center - from, Vector3.up).normalized;
            target.Push(direction * _settings.PushSpeed + Vector3.up * _settings.PushUpward);
            target.Fall(FallSeconds);
        }

        public void Begin()
        {
            SandboxClimber holder = Holder;
            SandboxClimber hanger = _crew.ClimberOf(HangerActor);
            holder.Teleport(_holderSpot.position + Vector3.up * StandHeight);
            hanger.Teleport(_edgeSpot.position + Vector3.up * StandHeight);

            _registry.Tie(holder.Actor, hanger.Actor);
            _registry.TryGet(holder.Actor, hanger.Actor, out RopeLink link);
            _registry.Grab(link);
            _registry.SetReel(link, _reel.ReelForLength(link, holder.Actor, _settings.CliffRopeLength));

            _pushTarget = hanger;
            _pushTimer = _settings.PushDelay;
        }

        public void BeginAutoLift()
        {
            Begin();
            _autoLift = true;
            _autoLiftWait = AutoLiftSettleSeconds;
        }

        public void Stop()
        {
            Trial.Stop();
            _pushTarget = null;
            _autoLift = false;
        }

        public void Tick(float deltaTime)
        {
            if (_pushTarget != null && !TickPushDelay(deltaTime))
                return;

            if (!Trial.IsRunning)
                return;

            SandboxClimber holder = Holder;

            if (!_registry.TryGet(holder.Actor, HangerActor, out RopeLink link))
            {
                Trial.Stop();
                return;
            }

            Trial.Tick(deltaTime, _forces.LoadOn(link, holder), _forces.PullOf(holder.Actor));
            TickAutoLift(link, deltaTime);
        }

        private bool TickPushDelay(float deltaTime)
        {
            _pushTimer -= deltaTime;

            if (_pushTimer > 0f)
                return false;

            Push(_pushTarget, _holderSpot.position);
            Trial.Begin(Holder, _pushTarget, _edgeSpot.position.y);
            _pushTarget = null;
            return true;
        }

        private void TickAutoLift(RopeLink link, float deltaTime)
        {
            if (!_autoLift)
                return;

            if (_autoLiftWait > 0f)
            {
                _autoLiftWait -= deltaTime;
                return;
            }

            _reelSelected(-1f, deltaTime);
            _autoLiftLogTimer -= deltaTime;

            if (_autoLiftLogTimer <= 0f)
            {
                _autoLiftLogTimer = AutoLiftLogInterval;
                Debug.Log($"progress t={Trial.Elapsed:0.0}s, rope={_reel.EffectiveLength(link):0.00}m | {Trial.Report()}");
            }

            if (!Trial.IsLifted && Trial.Elapsed < AutoLiftTimeout)
                return;

            Debug.Log(Trial.Report());
            _autoLift = false;
        }
    }
}
