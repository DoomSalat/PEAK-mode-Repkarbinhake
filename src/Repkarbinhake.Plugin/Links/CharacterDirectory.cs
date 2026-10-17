using System.Collections.Generic;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class CharacterDirectory
    {
        private readonly Dictionary<int, Character> _byActor = new Dictionary<int, Character>();

        public int LocalActor => Character.localCharacter == null ? 0 : ActorOf(Character.localCharacter);

        public void Refresh()
        {
            _byActor.Clear();

            foreach (Character character in Character.AllCharacters)
            {
                if (character == null || character.isBot)
                    continue;

                int actor = ActorOf(character);

                if (actor != 0)
                    _byActor[actor] = character;
            }
        }

        public bool TryFind(int actor, out Character result)
        {
            return _byActor.TryGetValue(actor, out result) && result != null;
        }

        public int ActorOf(Character character)
        {
            return character.photonView.Owner == null ? 0 : character.photonView.Owner.ActorNumber;
        }

        public bool IsActive(Character character)
        {
            return !character.data.dead;
        }
    }
}
