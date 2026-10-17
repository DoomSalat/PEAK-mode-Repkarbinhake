using System;
using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;

namespace Repkarbinhake.Core.Physics
{
    public sealed class StabilityResolver
    {
        private readonly IRopeSettings _settings;
        private readonly RopeHandPolicy _policy;
        private readonly IFootingSource _footing;
        private readonly HashSet<int> _visited = new HashSet<int>();
        private readonly HashSet<int> _group = new HashSet<int>();
        private readonly Queue<int> _frontier = new Queue<int>();

        public StabilityResolver(IRopeSettings settings, RopeHandPolicy policy, IFootingSource footing)
        {
            _settings = settings;
            _policy = policy;
            _footing = footing;
        }

        public float Resolve(int actor, IReadOnlyCollection<RopeLink> links)
        {
            if (!IsBracing(actor, links))
                return 0f;

            int otherBracing = CountBracingInGroup(actor, links) - 1;
            float groupFactor = MathF.Pow(1f - _settings.GroupBonus, otherBracing);
            _visited.Clear();
            float looseShare = (1f - ResolveChain(actor, links)) * groupFactor;
            return MathF.Min(1f - looseShare, _settings.MaxStability);
        }

        private float ResolveChain(int actor, IReadOnlyCollection<RopeLink> links)
        {
            _visited.Add(actor);
            float looseShare = 1f - _settings.BraceResistance;

            foreach (RopeLink link in links)
            {
                if (!link.Contains(actor))
                    continue;

                int supporter = link.Partner(actor);

                if (!link.IsHeldBy(supporter) || !_policy.CanBrace(_footing.FootingOf(supporter)) || _visited.Contains(supporter))
                    continue;

                looseShare *= 1f - _settings.ChainSupport * ResolveChain(supporter, links);
            }

            return 1f - looseShare;
        }

        private int CountBracingInGroup(int actor, IReadOnlyCollection<RopeLink> links)
        {
            _group.Clear();
            _frontier.Clear();
            _group.Add(actor);
            _frontier.Enqueue(actor);

            while (_frontier.Count > 0)
            {
                int current = _frontier.Dequeue();

                foreach (RopeLink link in links)
                {
                    if (link.Contains(current) && _group.Add(link.Partner(current)))
                        _frontier.Enqueue(link.Partner(current));
                }
            }

            int bracing = 0;

            foreach (int member in _group)
            {
                if (IsBracing(member, links))
                    bracing++;
            }

            return bracing;
        }

        private bool IsBracing(int actor, IReadOnlyCollection<RopeLink> links)
        {
            if (!_policy.CanBrace(_footing.FootingOf(actor)))
                return false;

            foreach (RopeLink link in links)
            {
                if (link.IsHeldBy(actor))
                    return true;
            }

            return false;
        }
    }
}
