using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxLives : IActorLifeSource
    {
        private readonly IReadOnlyList<SandboxClimber> _climbers;

        public SandboxLives(IReadOnlyList<SandboxClimber> climbers)
        {
            _climbers = climbers;
        }

        public bool IsDead(int actor)
        {
            return _climbers[actor - 1].IsDead;
        }
    }
}
