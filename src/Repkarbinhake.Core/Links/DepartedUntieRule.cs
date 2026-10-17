using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class DepartedUntieRule
    {
        public void Collect(IReadOnlyCollection<RopeLink> links, ICollection<int> presentActors, List<RopeLink> result)
        {
            result.Clear();

            foreach (RopeLink link in links)
            {
                if (!presentActors.Contains(link.FirstActor) || !presentActors.Contains(link.SecondActor))
                    result.Add(link);
            }
        }
    }
}
