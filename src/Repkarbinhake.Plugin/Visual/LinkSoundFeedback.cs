using Repkarbinhake.Core.Data;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class LinkSoundFeedback
    {
        private readonly CarabinerSounds _sounds;
        private readonly ActorStates _actors;

        public LinkSoundFeedback(CarabinerSounds sounds, ActorStates actors)
        {
            _sounds = sounds;
            _actors = actors;
        }

        public void Tied(RopeLink link)
        {
            if (TryMiddle(link, out Vector3 middle))
                _sounds.PlayLock(middle);
        }

        public void Untied(RopeLink link)
        {
            if (TryMiddle(link, out Vector3 middle))
                _sounds.PlayUnlock(middle);
        }

        private bool TryMiddle(RopeLink link, out Vector3 middle)
        {
            middle = Vector3.zero;

            if (!_actors.TryGetRopePoint(link.FirstActor, false, out Vector3 first) || !_actors.TryGetRopePoint(link.SecondActor, false, out Vector3 second))
                return false;

            middle = (first + second) / 2f;
            return true;
        }
    }
}
