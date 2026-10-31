using dot_battle_signal_bus.scripts.Core.Story;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Save
{
    public class InvestigationSave
    {
        public string CurrentCaseId;

        public List<string> EvidenceCollected;
        public List<string> WitnessesInterviewed;

        public List<string> Notes;

        public List<Coordinates> Coords;
    }
}
