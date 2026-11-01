using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Save
{
    public class GlobalProgress
    {
        public HashSet<string> CompletedScenarios;
        public HashSet<string> UnlockedScenarios;

        public HashSet<string> UnlockedWitnesses;
        public HashSet<string> DiscoveredEvidence;

        public Dictionary<string, bool> Flags;
        public Dictionary<string, int> Variables;
    }
}
