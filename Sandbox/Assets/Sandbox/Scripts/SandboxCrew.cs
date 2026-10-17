using System.Collections.Generic;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxCrew
    {
        private readonly SandboxClimber[] _climbers;
        private readonly List<SandboxClimber> _present = new List<SandboxClimber>();

        public SandboxCrew(SandboxClimber[] climbers)
        {
            _climbers = climbers;
        }

        public IReadOnlyList<SandboxClimber> All => _climbers;

        public SandboxClimber ClimberOf(int actor)
        {
            return _climbers[actor - 1];
        }

        public List<SandboxClimber> Present()
        {
            _present.Clear();

            foreach (SandboxClimber climber in _climbers)
            {
                if (climber.IsPresent)
                    _present.Add(climber);
            }

            return _present;
        }
    }
}
