using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Save
{
    public class MVPSaveData
    {
        public SaveLocation SaveLocation;

        public string ActiveScenarioId;

        public HashSet<string> Flags;

        public Dictionary<string, int> Variables;

        public List<string> EvidenceCollected;

        public List<string> WitnessesInterviewed;

        public List<string> Notes;

        public ScenarioSave ScenarioData;
    }
}
