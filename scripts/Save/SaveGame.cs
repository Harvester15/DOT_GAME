using dot_battle_signal_bus.scripts.Story;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Save
{
    public class SaveGame
    {
        public SaveLocation SaveLocation;

        public InvestigationSave Investigation;

        public ScenarioSave Scenario;

        public GlobalProgress Progress;
    }
}
