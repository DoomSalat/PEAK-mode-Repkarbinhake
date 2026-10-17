using System;
using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;

namespace Repkarbinhake.Core.Physics
{
    public sealed class TeamPull
    {
        public const float FirstHelperBonus = 0.6f;
        public const float HelperFalloff = 2f / 3f;

        private readonly RopeHandPolicy _policy;
        private readonly IFootingSource _footing;

        public TeamPull(RopeHandPolicy policy, IFootingSource footing)
        {
            _policy = policy;
            _footing = footing;
        }

        public int HelpersFor(RopeLink link, int actor, IReadOnlyCollection<RopeLink> links)
        {
            int load = link.Partner(actor);
            int helpers = 0;

            foreach (RopeLink other in links)
            {
                if (other == link || !other.Contains(load))
                    continue;

                int helper = other.Partner(load);

                if (helper != actor && other.IsHeldBy(helper) && _policy.CanBrace(_footing.FootingOf(helper)))
                    helpers++;
            }

            return helpers;
        }

        public float Multiplier(int helpers)
        {
            if (helpers <= 0)
                return 1f;

            return 1f + FirstHelperBonus * (1f - MathF.Pow(HelperFalloff, helpers)) / (1f - HelperFalloff);
        }

        public float SharedLoad(RopeLink link, int actor, IReadOnlyCollection<RopeLink> links, float load)
        {
            return load / Multiplier(HelpersFor(link, actor, links));
        }
    }
}
