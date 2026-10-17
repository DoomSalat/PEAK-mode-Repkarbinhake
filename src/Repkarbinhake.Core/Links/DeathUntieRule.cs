using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class DeathUntieRule
    {
        private readonly IActorLifeSource _lives;

        public DeathUntieRule(IActorLifeSource lives)
        {
            _lives = lives;
        }

        public void Collect(IReadOnlyCollection<RopeLink> links, int localActor, bool isHost, List<RopeLink> result)
        {
            result.Clear();

            foreach (RopeLink link in links)
            {
                if (ShouldUntie(link.FirstActor, localActor, isHost) || ShouldUntie(link.SecondActor, localActor, isHost))
                    result.Add(link);
            }
        }

        private bool ShouldUntie(int actor, int localActor, bool isHost)
        {
            return _lives.IsDead(actor) && (actor == localActor || isHost);
        }
    }
}
