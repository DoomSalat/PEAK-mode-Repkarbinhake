using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxFooting : IFootingSource
    {
        private readonly IReadOnlyList<SandboxClimber> _climbers;

        public SandboxFooting(IReadOnlyList<SandboxClimber> climbers)
        {
            _climbers = climbers;
        }

        public Footing FootingOf(int actor)
        {
            return _climbers[actor - 1].Footing;
        }
    }
}
