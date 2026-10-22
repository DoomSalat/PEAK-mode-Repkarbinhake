using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Network;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class UntieWitness
    {
        private const float SnapShare = 0.9f;

        private readonly PhotonLinkTransport _transport;
        private readonly ActorStates _actors;
        private readonly CharacterDirectory _characters;
        private readonly IRopeSettings _settings;

        public UntieWitness(PhotonLinkTransport transport, ActorStates actors, CharacterDirectory characters, IRopeSettings settings)
        {
            _transport = transport;
            _actors = actors;
            _characters = characters;
            _settings = settings;
        }

        public bool TryGetUntier(RopeLink link, out int untier)
        {
            untier = _transport.DispatchingWriter;

            if (untier == 0 || !link.Contains(untier) || !IsPlayer(link.FirstActor) || !IsPlayer(link.SecondActor))
                return false;

            int partner = link.Partner(untier);
            return IsPresent(untier) && IsPresent(partner) && !Snapped(untier, partner);
        }

        private bool IsPlayer(int actor)
        {
            return actor > 0 && !AnchorActors.IsAnchor(actor);
        }

        private bool IsPresent(int actor)
        {
            return !_actors.IsDead(actor) && PhotonNetwork.CurrentRoom?.GetPlayer(actor) != null;
        }

        private bool Snapped(int first, int second)
        {
            if (!_characters.TryFind(first, out Character firstCharacter) || !_characters.TryFind(second, out Character secondCharacter))
                return false;

            float reach = _settings.SnapDistance * SnapShare;
            return (firstCharacter.Center - secondCharacter.Center).sqrMagnitude > reach * reach;
        }
    }
}
