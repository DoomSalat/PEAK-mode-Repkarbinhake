using System.Collections.Generic;
using BepInEx.Logging;
using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Debugging;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Input;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Network;
using Repkarbinhake.Plugin.Patches;
using Repkarbinhake.Plugin.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin
{
    public sealed class RepkaBack : MonoBehaviour
    {
        private const float MaxVisualDeltaTime = 1f / 30f;

        private readonly List<RopeLink> _deadLinks = new List<RopeLink>();
        private RepkaConfig _config;
        private LinkRegistry _registry;
        private DeathUntieRule _deathRule;
        private RopeHandPolicy _policy;
        private CharacterDirectory _characters;
        private ActorStates _actors;
        private KeyBinding _linkKey;
        private KeyBinding _anchorKey;
        private KeyBinding _pullKey;
        private KeyBinding _unwindKey;
        private HostSwitchInput _hookSwitch;
        private HostSwitchInput _lockSwitch;
        private TestDummies _dummies;
        private LinkHoldInput _linkHold;
        private AnchorInput _anchorInput;
        private AnchorSync _anchorSync;
        private AnchorRopes _anchorRopes;
        private RopeHands _hands;
        private ReelDriver _reelDriver;
        private AutoHandUp _handUp;
        private RopeHoldPoses _poses;
        private LinkColors _colors;
        private SpoolProps _spools;
        private RopeForces _forces;
        private RopeRescue _rescue;
        private HangRecovery _hangRecovery;
        private RopeKnockdown _knockdown;
        private RepkaHookFeed _hookFeed;
        private RopePresenterPool _ropes;
        private BeltCarabiners _belts;
        private RoomLinkSync _sync;
        private ModVersionCheck _versionCheck;
        private RopePathSync _pathSync;
        private RopeColliderFilter _colliderFilter;
        private LinkSoundFeedback _sounds;
        private HostFlag _linkLock;
        private HostFlag _hookRule;
        private UntieWitness _untieWitness;
        private RopeEventLog _eventLog;
        private IDebugTools _debug;
        private ConfigWindowInput _configWindow;

        private void OnEnable()
        {
            _registry.LinkAdded += OnLinkAdded;
            _registry.LinkRemoved += OnLinkRemoved;
            _ropes.RopeInteracted += OnRopeInteracted;
            _linkHold.Untied += OnLocalUntied;
            _linkLock.Changed += OnLinkLockChanged;
            _hookRule.Changed += OnHookRuleChanged;
            _sync.Enable();
            _anchorSync.Enable();
            _versionCheck.Enable();
            _pathSync.Enable();
        }

        private void OnDisable()
        {
            if (_registry != null)
            {
                _registry.LinkAdded -= OnLinkAdded;
                _registry.LinkRemoved -= OnLinkRemoved;
            }

            if (_ropes != null)
                _ropes.RopeInteracted -= OnRopeInteracted;

            if (_linkHold != null)
                _linkHold.Untied -= OnLocalUntied;

            if (_linkLock != null)
                _linkLock.Changed -= OnLinkLockChanged;

            if (_hookRule != null)
                _hookRule.Changed -= OnHookRuleChanged;

            _sync?.Disable();
            _anchorSync?.Disable();
            _versionCheck?.Disable();
            _pathSync?.Disable();
            _hookFeed.Clear();
        }

        private void OnDestroy()
        {
            _linkKey?.Dispose();
            _anchorKey?.Dispose();
            _pullKey?.Dispose();
            _unwindKey?.Dispose();
            _hookSwitch?.Dispose();
            _lockSwitch?.Dispose();
            _debug?.Dispose();
            _configWindow?.Dispose();
        }

        private void Update()
        {
            _configWindow.Tick();
            _sync.Tick(Time.deltaTime, Time.time);
            Character local = Character.localCharacter;

            if (!PhotonNetwork.InRoom || local == null || MainCamera.instance == null)
            {
                _linkHold.Cancel();
                _poses.Clear();
                _anchorInput.Cancel();
                _handUp.Stop();
                _knockdown.Clear();
                _colliderFilter.Clear();
                _spools.Clear();
                _hookFeed.Clear();
                _belts.Clear();
                _eventLog.Clear();
                _debug.Hide();
                return;
            }

            _characters.Refresh();
            int actor = _registry.LocalActor;
            Footing footing = _actors.FootingOf(actor);
            Transform view = MainCamera.instance.transform;

            bool pulling = false;
            bool unwinding = false;
            bool holding = _hands.IsHolding(actor);

            if (_configWindow.IsOpen)
            {
                _anchorInput.Cancel();
                _linkHold.Cancel();
            }
            else
            {
                _anchorInput.Tick(local, actor, footing, view, holding && _config.UnwindKey.Value == _config.AnchorKey.Value);
                _debug.TickInput(actor, view);
                _hookSwitch.Tick();
                _lockSwitch.Tick();
                _linkHold.Tick(local, actor, footing, view, holding, Time.deltaTime);
                pulling = _pullKey.IsPressed && !_linkHold.HasAction;
                unwinding = _unwindKey.IsPressed;
            }

            _hands.Enforce(local, actor, footing, Time.deltaTime);
            _anchorRopes.Tick(local, actor, footing, Time.deltaTime);

            if (_hands.IsHolding(actor))
                _reelDriver.Tick(local, actor, footing, pulling, unwinding, Time.deltaTime);
            else
                _reelDriver.Stop();

            _handUp.Tick(local, actor, _reelDriver.IsReelingIn, Time.deltaTime);

            _pathSync.Tick(actor, Time.deltaTime);
            _knockdown.Tick(local, actor, Time.deltaTime);
            _poses.Refresh(Time.deltaTime);
            UntieDead(actor);
            _hookFeed.Publish(actor, footing, view, _hangRecovery.IsHanging);
            _eventLog.Flush();
            _debug.ShowOverlay(local, actor);
        }

        private void FixedUpdate()
        {
            Character local = Character.localCharacter;

            if (!PhotonNetwork.InRoom || local == null || local.data.dead)
                return;

            _forces.Apply(local, _registry.LocalActor);
            _rescue.Tick(local, _forces.IsPulledByTeammate);
            _hangRecovery.Tick(local, _actors.FootingOf(_registry.LocalActor), _forces.IsOnTautRope, _forces.IsPulledByTeammate);
            _forces.ApplyToDummies(_dummies);
        }

        private void LateUpdate()
        {
            if (!PhotonNetwork.InRoom)
                return;

            _spools.Render();
            _belts.Tick(Mathf.Min(Time.deltaTime, MaxVisualDeltaTime));
            _ropes.Render(Mathf.Min(Time.deltaTime, MaxVisualDeltaTime));
        }

        public void Initialize(RepkaConfig config, RepkaAssets assets, ManualLogSource log)
        {
            _config = config;
            LobbyRopeSettings settings = new LobbyRopeSettings(config);
            _policy = new RopeHandPolicy();
            PhotonLinkTransport transport = new PhotonLinkTransport();
            _registry = new LinkRegistry(transport);
            _colors = new LinkColors(_registry);
            RepkaHooks.Badges = new PlayerListBadges(_colors, assets.ReticleSprite, log);
            RepkaHooks.HangingCamera = new HangingCamera();
            RepkaHooks.Hud = new CarabinerHud(assets.ReticleSprite, new HookIndicatorView(assets.HookIndicatorSprite, assets.HookRetractSprite), log);
            _characters = new CharacterDirectory();
            _dummies = new TestDummies();
            VanillaRopeSpool spool = new VanillaRopeSpool();
            RepkaHooks.Spool = spool;
            AnchorVisuals anchorVisuals = new AnchorVisuals(spool);
            AnchorBoard anchors = new AnchorBoard(_registry, anchorVisuals);
            _poses = new RopeHoldPoses(_registry, _characters);
            RepkaHooks.HoldPoses = _poses.Poses;
            _spools = new SpoolProps(transform, spool, _poses, _registry, _colors, config);
            _actors = new ActorStates(_characters, anchors, _spools, _dummies);
            _deathRule = new DeathUntieRule(_actors);
            StabilityResolver stability = new StabilityResolver(settings, _policy, _actors);
            TensionSolver tension = new TensionSolver(settings);
            RopeSettingsOverride anchorSettings = new RopeSettingsOverride(settings, config.AnchorStiffness, config.AnchorDamping, config.AnchorMaxPull);
            TensionSolver anchorTension = new TensionSolver(anchorSettings);
            TensionSolver haulTension = new TensionSolver(new RopeSettingsOverride(settings, config.HaulStiffness, null, null));
            RopeReel reel = new RopeReel(settings, _policy);
            _colliderFilter = new RopeColliderFilter();
            PhysicsRopeObstacles obstacles = new PhysicsRopeObstacles(HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainMap), transform, _colliderFilter);
            HookLengths hookLengths = new HookLengths();
            LinkLengths lengths = new LinkLengths(reel, hookLengths);
            _belts = new BeltCarabiners(transform, assets.Carabiner, new CarabinerMaterials(assets.KnurlTexture), _registry, _characters, _colors, config.BeltCarabiners, config.ShowOwnCarabiners);
            _ropes = new RopePresenterPool(transform, config, lengths, new RopeSegmentation(config), _actors, _registry, _policy, obstacles, spool, _belts);

            _linkKey = new KeyBinding(config.LinkKey);
            _anchorKey = new KeyBinding(config.AnchorKey);
            _pullKey = new KeyBinding(config.PullKey);
            _unwindKey = new KeyBinding(config.UnwindKey);
            _hookSwitch = new HostSwitchInput(new KeyBinding(config.AllowHooksKey), config.AllowHooks);
            _lockSwitch = new HostSwitchInput(new KeyBinding(config.LockLinksKey), config.LockLinks);
            _linkLock = new HostFlag(PhotonLinkTransport.LockLinksKey, config.LockLinks, false);
            _linkHold = new LinkHoldInput(_registry, new LinkTargetPicker(config, _dummies), _linkKey, settings, _linkLock);
            _hookRule = new HostFlag(PhotonLinkTransport.AllowHooksKey, config.AllowHooks, true);
            _anchorSync = new AnchorSync(anchors, _hookRule);
            _anchorInput = new AnchorInput(anchors, _anchorSync, anchorVisuals, spool, _anchorKey, settings, _hookRule);
            _anchorRopes = new AnchorRopes(_registry, anchors, _ropes, settings, config.HookRetractSpeed, config.HookLength, hookLengths);
            _hands = new RopeHands(_registry, _policy, _actors);
            TeamPull team = new TeamPull(_policy, _actors);
            _reelDriver = new ReelDriver(_registry, reel, tension, _ropes, team);
            _handUp = new AutoHandUp(_registry, _characters, _actors, _policy, config.AutoHandUp);
            LedgeAssist ledge = new LedgeAssist();
            MantleAssist mantle = new MantleAssist();
            _forces = new RopeForces(_registry, stability, tension, anchorTension, haulTension, lengths, _actors, _ropes, config, obstacles, ledge, mantle);
            _rescue = new RopeRescue(config.FreeWhenPulled);
            _hangRecovery = new HangRecovery(config.RecoverWhenHeld);
            _knockdown = new RopeKnockdown(_registry, reel, _ropes, config);
            _sync = new RoomLinkSync(transport, _registry, new UntiedKeySweeper(), new DepartedUntieRule(), settings, new[] { _hookRule, _linkLock }, config);
            _versionCheck = new ModVersionCheck(RepkaPlugin.Version, log);
            _pathSync = new RopePathSync(_registry, _ropes);
            _sounds = new LinkSoundFeedback(new CarabinerSounds(assets, config), _actors);
            _untieWitness = new UntieWitness(transport, _actors, _characters, settings);
            _eventLog = new RopeEventLog(log);
            HudFeed hudFeed = new HudFeed(_registry, _linkHold, anchors, _anchorRopes, config.LinkKey, config.PullKey, config.UnwindKey);
            HeldRopeFeed heldRopeFeed = new HeldRopeFeed(_registry, reel, _policy, _ropes, _linkHold, _colors, config.RopeColor);
            _hookFeed = new RepkaHookFeed(hudFeed, heldRopeFeed, new AssistFeed(_registry, _handUp, config.StableCamera));
            RepkaHooks.GaugeTint = new RopeGaugeTint();
            ConfigEditorWindow window = gameObject.AddComponent<ConfigEditorWindow>();
            window.Initialize(config.File);
            _configWindow = new ConfigWindowInput(window, new KeyBinding(config.ConfigEditorKey));
            _debug = BuildFlavor.HasDebugTools ? CreateDebugTools(config, log, new RopeDebugReport(_registry, _actors, stability, tension, reel, lengths, team, ledge, mantle, _ropes, settings, anchorSettings)) : new NoDebugTools();
        }

        private void UntieDead(int actor)
        {
            _deathRule.Collect(_registry.Links, actor, PhotonNetwork.IsMasterClient, _deadLinks);

            foreach (RopeLink link in _deadLinks)
            {
                if (!AnchorActors.IsAnchor(link.FirstActor))
                    _registry.Untie(link);
            }
        }

        private IDebugTools CreateDebugTools(RepkaConfig config, ManualLogSource log, RopeDebugReport report)
        {
            KeyBinding dummyKey = new KeyBinding(config.TestDummyKey);
            TestDummyInput dummyInput = new TestDummyInput(_registry, _dummies, dummyKey, config.TestDummyCount, log);
            return new DebugTools(config, report, gameObject.AddComponent<RopeDebugOverlay>(), dummyKey, dummyInput);
        }

        private void OnLocalUntied(int partner)
        {
            _eventLog.Untied(_registry.LocalActor, partner);
        }

        private void OnLinkLockChanged(bool locked)
        {
            _eventLog.TiesChanged(locked);
        }

        private void OnHookRuleChanged(bool allowed)
        {
            _eventLog.HooksChanged(allowed);
        }

        private void OnRopeInteracted(RopeLink link)
        {
            _hands.Toggle(Character.localCharacter, link);
        }

        private void OnLinkAdded(RopeLink link)
        {
            _colors.Assign(link);
            _ropes.Add(link);

            if (_colors.TryGetColor(link, out Color color))
                _ropes.Tint(link, color);

            if (!_sync.IsReadingSnapshot)
                _sounds.Tied(link);
        }

        private void OnLinkRemoved(RopeLink link)
        {
            if (_untieWitness.TryGetUntier(link, out int untier))
                _eventLog.Untied(untier, link.Partner(untier));

            _pathSync.Forget(link);
            _knockdown.Forget(link);
            _colors.Release(link);
            _sounds.Untied(link);
            _ropes.Remove(link);
        }
    }
}
