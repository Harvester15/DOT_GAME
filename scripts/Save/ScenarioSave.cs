
using dot_battle_signal_bus.scripts.Actors.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Save
{
    public class ScenarioSave
    {
        public string ScenarioId;

        public int CurrentTurn;

        public Hero Hero; //might have more than one hero in the future, but for now we only have one

        public List<ArmyStack> Army;

        public List<string> CapturedLocations; //if these are cities, villages or resources, we can use the location id to reference them

        public List<string> ScenarioFlags; //um?
    }
}
