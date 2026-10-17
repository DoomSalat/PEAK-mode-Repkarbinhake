using Repkarbinhake.Core.Data;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class ActorStates : IFootingSource, IActorLifeSource
    {
        private readonly CharacterDirectory _characters;
        private readonly TestAnchor _anchor;
        private readonly TestDummy _dummy;
        private readonly SpoolProps _spools;

        public ActorStates(CharacterDirectory characters, TestAnchor anchor, TestDummy dummy, SpoolProps spools)
        {
            _characters = characters;
            _anchor = anchor;
            _dummy = dummy;
            _spools = spools;
        }

        public Footing FootingOf(int actor)
        {
            if (actor == TestAnchor.Actor)
                return Footing.Grounded;

            if (actor == TestDummy.Actor)
                return _dummy.IsPlaced && _dummy.IsGrounded ? Footing.Grounded : Footing.Airborne;

            if (!_characters.TryFind(actor, out Character character))
                return Footing.Airborne;

            CharacterData data = character.data;

            if (data.dead || data.passedOut || data.fullyPassedOut)
                return Footing.Unconscious;

            if (data.isClimbingAnything)
                return Footing.Climbing;

            return data.isGrounded ? Footing.Grounded : Footing.Airborne;
        }

        public bool IsDead(int actor)
        {
            return actor > 0 && _characters.TryFind(actor, out Character character) && character.data.dead;
        }

        public bool TryGetRopePoint(int actor, bool holding, out Vector3 point)
        {
            if (actor == TestAnchor.Actor)
            {
                point = _anchor.Position;
                return _anchor.IsPlaced;
            }

            if (actor == TestDummy.Actor)
            {
                point = _dummy.IsPlaced ? _dummy.RopePoint : Vector3.zero;
                return _dummy.IsPlaced;
            }

            if (!_characters.TryFind(actor, out Character character))
            {
                point = Vector3.zero;
                return false;
            }

            if (holding && _spools.TryGetRopeExit(character, out point, out _))
                return true;

            point = character.GetBodypart(holding ? BodypartType.Hand_R : BodypartType.Hip).transform.position;
            return true;
        }

        public bool TryGetRopeLead(int actor, out Vector3 lead)
        {
            lead = Vector3.zero;
            return actor > 0 && _characters.TryFind(actor, out Character character) && _spools.TryGetRopeExit(character, out _, out lead);
        }

        public bool IsPresent(int actor)
        {
            if (actor == TestAnchor.Actor)
                return _anchor.IsPlaced;

            return actor == TestDummy.Actor ? _dummy.IsPlaced : _characters.TryFind(actor, out _);
        }
    }
}
