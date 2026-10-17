using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Tests.Fakes
{
    public sealed class FakeFooting : IFootingSource
    {
        private readonly Dictionary<int, Footing> _overrides = new Dictionary<int, Footing>();

        public void Set(int actor, Footing footing)
        {
            _overrides[actor] = footing;
        }

        public Footing FootingOf(int actor)
        {
            return _overrides.TryGetValue(actor, out Footing footing) ? footing : Footing.Grounded;
        }
    }
}
