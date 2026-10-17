using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxRopeBack : MonoBehaviour
    {
        private const float MaxVisualDeltaTime = 1f / 30f;
        private const float FallSeconds = 2.5f;
        private const float KnockOutSeconds = 6f;
        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";
        private const string Horizontal = "Horizontal";
        private const string Vertical = "Vertical";

        [SerializeField] private SandboxRopeSettings _settings;
        [SerializeField] private SandboxClimber[] _climbers;
        [SerializeField] private SandboxCamera _camera;
        [SerializeField] private SandboxDebugOverlay _overlay;
        [SerializeField] private CarabinerAudio _audio;
        [SerializeField] private Material _ropeMaterial;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Transform _cliffHolderSpot;
        [SerializeField] private Transform _cliffEdgeSpot;
        [SerializeField] private Color _highlightEmission = new Color(0.55f, 0.4f, 0.1f);
        [SerializeField] private SandboxClimberColors _colors = new SandboxClimberColors();

        private LinkRegistry _registry;
        private RopeHandPolicy _policy;
        private RopeReel _reel;
        private SandboxCrew _crew;
        private SandboxAimPicker _aimPicker;
        private SandboxRopeVisuals _visuals;
        private SandboxRopeForces _forces;
        private SandboxLifeRules _rules;
        private SandboxOverlayReport _report;
        private SandboxClimberTint _tint;
        private SandboxCliffScenario _scenario;
        private HoldAction _linkHold;
        private bool _linkHoldUnties;
        private SandboxClimber _selected;
        private SandboxClimber _aimed;
        private RopeLink _aimedRope;
        private Vector3 _moveInput;
        private bool _jumpRequested;

        private void Awake()
        {
            _registry = new LinkRegistry(new LoopbackLinkTransport());
            _policy = new RopeHandPolicy();
            _crew = new SandboxCrew(_climbers);
            StabilityResolver stability = new StabilityResolver(_settings, _policy, new SandboxFooting(_climbers));
            TensionSolver tension = new TensionSolver(_settings);
            RopeSegmentation segmentation = new RopeSegmentation(_settings);
            _reel = new RopeReel(_settings, _policy);
            _aimPicker = new SandboxAimPicker(_groundMask);
            _visuals = new SandboxRopeVisuals(_crew, _settings, _reel, segmentation, new PhysicsRopeObstacles(_groundMask, transform, new NoColliderFilter()), _ropeMaterial, _highlightEmission);
            _forces = new SandboxRopeForces(_registry, stability, tension, _reel, _visuals, _settings);
            _rules = new SandboxLifeRules(_registry, _policy, tension, _crew);
            _report = new SandboxOverlayReport(_registry, stability, _reel, segmentation, _settings, _crew, _visuals, _forces, _rules);
            _tint = new SandboxClimberTint(_colors);
            _scenario = new SandboxCliffScenario(_registry, _reel, _forces, _settings, _crew, _cliffHolderSpot, _cliffEdgeSpot, ReelSelected);
            _linkHold = new HoldAction();

            for (int i = 0; i < _climbers.Length; i++)
                _climbers[i].Initialize(i + 1, _groundMask);

            Select(_climbers[0]);
            _camera.Snap(_selected.Center);
            LockCursor();
        }

        private void OnEnable()
        {
            _registry.LinkAdded += OnLinkAdded;
            _registry.LinkRemoved += OnLinkRemoved;
        }

        private void OnDisable()
        {
            if (_registry == null)
                return;

            _registry.LinkAdded -= OnLinkAdded;
            _registry.LinkRemoved -= OnLinkRemoved;
        }

        private void Update()
        {
            ReadSelection();
            ReadCursor();
            ReadMovement();
            ReadHotkeys();
            UpdateAim();
            ReadRopeActions();
            ReadReel();
        }

        private void FixedUpdate()
        {
            foreach (SandboxClimber climber in _crew.Present())
            {
                climber.Move(climber == _selected ? _moveInput : Vector3.zero);
                climber.Tick(Time.fixedDeltaTime);
            }

            if (_jumpRequested)
                _selected.Jump();

            _jumpRequested = false;
            _rules.Apply(_selected.Actor);
            _forces.Apply(_crew.Present());
            _scenario.Tick(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            _camera.Follow(_selected.Center, Time.deltaTime);
            _visuals.Render(_registry.Links, Mathf.Min(Time.deltaTime, MaxVisualDeltaTime));

            foreach (SandboxClimber climber in _climbers)
                _tint.Apply(climber, climber == _selected, _rules.IsHolding(climber.Actor));

            _overlay.Show(_report.Build(_selected, _scenario.Trial));
        }

        private void ReadSelection()
        {
            for (int i = 0; i < _climbers.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) && _climbers[i].IsPresent)
                    Select(_climbers[i]);
            }
        }

        private void ReadCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                UnlockCursor();
            else if (Input.GetMouseButtonDown(0))
                LockCursor();

            if (Cursor.lockState == CursorLockMode.Locked)
                _camera.Look(new Vector2(Input.GetAxis(MouseX), Input.GetAxis(MouseY)));
        }

        private void ReadMovement()
        {
            _moveInput = _camera.FlatRight * Input.GetAxisRaw(Horizontal) + _camera.FlatForward * Input.GetAxisRaw(Vertical);

            if (Input.GetKeyDown(KeyCode.Space))
                _jumpRequested = true;
        }

        private void ReadHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.L))
                LeaveSelected();

            if (Input.GetKeyDown(KeyCode.K))
                _selected.Kill();

            if (Input.GetKeyDown(KeyCode.U))
                _selected.KnockOut(KnockOutSeconds);

            if (Input.GetKeyDown(KeyCode.F))
                _selected.Fall(FallSeconds);

            if (Input.GetKeyDown(KeyCode.R))
                RespawnAll();

            if (Input.GetKeyDown(KeyCode.P) && _aimed != null)
                _scenario.Push(_aimed, _selected.Center);

            if (Input.GetKeyDown(KeyCode.T))
                StartCliffScenario(false);
        }

        private void UpdateAim()
        {
            bool hasTarget = _aimPicker.TryPick(_selected, _climbers, _camera.Position, _camera.Forward, _settings.LinkDistance, _settings.LinkAimAngle, out _aimed);
            _aimedRope = _visuals.PickOwn(_registry.Links, _selected.Actor, _camera.Position, _camera.Forward);
            _visuals.Highlight(_aimedRope);

            if (hasTarget)
            {
                string action = _registry.TryGet(_selected.Actor, _aimed.Actor, out _) ? "отцепиться от" : "связаться с";
                _overlay.ShowCarabiner($"Держать G — {action} #{_aimed.Actor}", _linkHold.Progress);
            }
            else if (_aimedRope != null)
                _overlay.ShowHint($"E — {(_aimedRope.IsHeldBy(_selected.Actor) ? "отпустить" : "взять")} верёвку #{_aimedRope.FirstActor}—#{_aimedRope.SecondActor}");
            else
                _overlay.HideCarabiner();
        }

        private void ReadRopeActions()
        {
            if (Input.GetKeyDown(KeyCode.G) && _aimed != null)
                BeginLinkHold(_aimed);

            int aimedActor = _aimed == null ? HoldAction.NoTarget : _aimed.Actor;

            if (_linkHold.Advance(Input.GetKey(KeyCode.G), aimedActor, Time.deltaTime))
                CompleteLinkHold(aimedActor);

            if (Input.GetKeyDown(KeyCode.E))
                ToggleHold();
        }

        private void BeginLinkHold(SandboxClimber partner)
        {
            _linkHoldUnties = _registry.TryGet(_selected.Actor, partner.Actor, out _);
            _linkHold.Begin(partner.Actor, _linkHoldUnties ? _settings.UntieHoldSeconds : _settings.TieHoldSeconds);
        }

        private void CompleteLinkHold(int partnerActor)
        {
            bool isLinked = _registry.TryGet(_selected.Actor, partnerActor, out RopeLink link);

            if (_linkHoldUnties && isLinked)
                _registry.Untie(link);
            else if (!_linkHoldUnties && !isLinked)
                _registry.Tie(_selected.Actor, partnerActor);
        }

        private void ToggleHold()
        {
            if (!_policy.CanHold(_selected.Footing))
                return;

            if (_aimedRope == null)
            {
                _registry.ReleaseAllHeld();
                return;
            }

            if (_aimedRope.IsHeldBy(_selected.Actor))
            {
                _registry.Release(_aimedRope);
                return;
            }

            _registry.ReleaseAllHeld();
            _registry.Grab(_aimedRope);
        }

        private void ReadReel()
        {
            float notches = Input.mouseScrollDelta.y;

            if (!Mathf.Approximately(notches, 0f))
                ReelSelected(Mathf.Sign(notches), Mathf.Abs(notches) * _settings.SecondsPerScrollNotch);
        }

        private void ReelSelected(float direction, float seconds)
        {
            if (direction < 0f)
                _scenario.Trial.MarkLiftStart();

            foreach (RopeLink link in _registry.Links)
            {
                if (!link.IsHeldBy(_selected.Actor))
                    continue;

                ReelStep step = _reel.StepReel(link, _selected.Actor, direction, seconds, _forces.LoadOn(link, _selected), _selected.Footing, _selected.Stamina);
                _selected.UseStamina(step.StaminaCost);
                _scenario.Trial.AddStaminaSpent(_selected.Actor, step.StaminaCost);
                _registry.SetReel(link, step.Reel);
            }
        }

        [ContextMenu(nameof(RunCliffAutoLift))]
        private void RunCliffAutoLift()
        {
            StartCliffScenario(true);
        }

        private void StartCliffScenario(bool autoLift)
        {
            foreach (SandboxClimber climber in _climbers)
                _registry.UntieActor(climber.Actor);

            RespawnAll();
            Select(_scenario.Holder);

            if (autoLift)
                _scenario.BeginAutoLift();
            else
                _scenario.Begin();

            _camera.Snap(_selected.Center);
        }

        private void LeaveSelected()
        {
            _selected.Leave();

            foreach (SandboxClimber climber in _crew.Present())
            {
                Select(climber);
                return;
            }
        }

        private void Select(SandboxClimber climber)
        {
            _selected = climber;
            _registry.Bind(climber.Actor);
        }

        private void RespawnAll()
        {
            _scenario.Stop();

            foreach (SandboxClimber climber in _climbers)
            {
                climber.Rejoin();
                climber.Respawn();
            }
        }

        private void OnLinkAdded(RopeLink link)
        {
            _visuals.Add(link);
            _audio.PlayLock(Midpoint(link));
        }

        private void OnLinkRemoved(RopeLink link)
        {
            _visuals.Remove(link);
            _audio.PlayUnlock(Midpoint(link));
        }

        private Vector3 Midpoint(RopeLink link)
        {
            return (_crew.ClimberOf(link.FirstActor).RopePoint + _crew.ClimberOf(link.SecondActor).RopePoint) / 2f;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
