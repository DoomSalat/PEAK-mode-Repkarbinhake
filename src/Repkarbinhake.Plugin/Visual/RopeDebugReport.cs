using System.Collections.Generic;
using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeDebugReport
    {
        private const float Percent = 100f;
        private const string AnchorName = "крюк";
        private const string DummyName = "капсула";
        private const string UnknownName = "?";

        private readonly LinkRegistry _registry;
        private readonly ActorStates _actors;
        private readonly StabilityResolver _stability;
        private readonly TensionSolver _tension;
        private readonly TensionSolver _anchorTension;
        private readonly RopeReel _reel;
        private readonly LinkLengths _lengths;
        private readonly TeamPull _team;
        private readonly LedgeAssist _ledge;
        private readonly MantleAssist _mantle;
        private readonly RopePresenterPool _ropes;
        private readonly IRopeSettings _settings;
        private readonly IRopeSettings _anchorSettings;
        private readonly List<string> _lines = new List<string>();

        public RopeDebugReport(LinkRegistry registry, ActorStates actors, StabilityResolver stability, TensionSolver tension, RopeReel reel, LinkLengths lengths, TeamPull team, LedgeAssist ledge, MantleAssist mantle, RopePresenterPool ropes, IRopeSettings settings, IRopeSettings anchorSettings)
        {
            _registry = registry;
            _actors = actors;
            _stability = stability;
            _tension = tension;
            _reel = reel;
            _lengths = lengths;
            _team = team;
            _ledge = ledge;
            _mantle = mantle;
            _ropes = ropes;
            _settings = settings;
            _anchorSettings = anchorSettings;
            _anchorTension = new TensionSolver(anchorSettings);
        }

        public IReadOnlyList<string> Build(Character local, int actor)
        {
            _lines.Clear();
            float stability = _stability.Resolve(actor, _registry.Links);
            Vector3 velocity = local.data.avarageVelocity;
            _lines.Add($"ТЫ  actor {actor}  опора {_actors.FootingOf(actor)}  устойчивость {stability * Percent:0}%  скорость {velocity.magnitude:0.0} (верт {velocity.y:0.0})  в воздухе {local.data.sinceGrounded:0.00} с  выносливость {local.GetTotalStamina() * Percent:0}%");
            _lines.Add($"язык игры: {(int)LocalizedText.CURRENT_LANGUAGE} {LocalizedText.CURRENT_LANGUAGE}  подсказка «{RepkaTexts.Grab}»");
            _lines.Add($"уступ: застрял {_ledge.StuckFor:0.00} с{(_ledge.IsPushing ? "  ОТТАЛКИВАНИЕ" : string.Empty)}");
            _lines.Add($"на край: застрял {_mantle.StuckFor:0.00} с{(_mantle.IsPushing ? $"  ТОЛКАЮ к точке ({_mantle.Target.X:0.0}, {_mantle.Target.Y:0.0}, {_mantle.Target.Z:0.0})" : string.Empty)}");
            _lines.Add($"лобби  maxLength {_settings.MaxLength:0.0}  хост {PhotonNetwork.IsMasterClient}  твои силы: Stiffness {_settings.Stiffness:0}  Damping {_settings.Damping:0}  MaxPull {_settings.MaxPull:0}  ArmStrength {_settings.ArmStrength:0}");

            foreach (RopeLink link in _registry.Links)
            {
                if (link.Contains(actor))
                    AddOwnLink(link, actor, stability, velocity);
                else
                    _lines.Add($"{link.Key}  чужая  длина {_lengths.EffectiveLength(link):0.00}  держат {link.First.IsHeld}/{link.Second.IsHeld}");
            }

            return _lines;
        }

        private void AddOwnLink(RopeLink link, int actor, float stability, Vector3 velocity)
        {
            int partner = link.Partner(actor);
            float length = _lengths.EffectiveLength(link);
            RopeSpan span = _ropes.SpanFor(link, actor);
            float stretch = span.PathLength - length;
            bool hook = AnchorActors.IsAnchor(partner);
            TensionSolver tension = hook ? _anchorTension : _tension;
            float load = tension.Load(span, length);
            float pullOnYou = tension.Solve(span, velocity.ToNumerics(), length, stability).Length();
            float maxPull = hook ? _anchorSettings.MaxPull : _settings.MaxPull;
            bool capped = load >= maxPull;
            _lines.Add($"— {NameOf(partner)} ({link.Key})  держишь {link.IsHeldBy(actor)}  держит партнёр {link.IsHeldBy(partner)}  опора партнёра {_actors.FootingOf(partner)}");
            _lines.Add($"   длина {length:0.00}  путь {span.PathLength:0.00}  растяжение {stretch:0.00}  смотано тобой {link.EndOf(actor).Reel:0.00} / партнёром {link.EndOf(partner).Reel:0.00}  точек {_ropes.SegmentsOf(link)}");
            _lines.Add($"   на тебя: нагрузка {load:0.0}/{maxPull:0}{(capped ? " ПОТОЛОК" : string.Empty)}  тяга с учётом устойчивости {pullOnYou:0.0}");

            if (AnchorActors.IsAnchor(partner))
                return;

            float partnerStability = _stability.Resolve(partner, _registry.Links);
            float pullOnPartner = load * (1f - partnerStability);
            _lines.Add($"   на партнёра (оценка по твоим настройкам): устойчивость {partnerStability * Percent:0}%  тяга {pullOnPartner:0.0}  (решает его MaxPull)");

            if (!link.IsHeldBy(actor))
                return;

            int helpers = _team.HelpersFor(link, actor, _registry.Links);
            float shared = _team.SharedLoad(link, actor, _registry.Links, load);
            _lines.Add($"   сматывание: скорость {_reel.ReelInFactor(shared) * Percent:0}%  помощников {helpers} (×{_team.Multiplier(helpers):0.00})  нагрузка на руки {shared:0.0}/{_settings.ArmStrength:0}  выносливость {_reel.StaminaPerMeter(shared):0.000}/м");
        }

        private string NameOf(int actor)
        {
            if (AnchorActors.IsAnchor(actor))
                return AnchorName;

            if (actor <= TestDummies.FirstActor)
                return DummyName;

            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom?.GetPlayer(actor);
            return player == null ? UnknownName : player.NickName;
        }
    }
}
