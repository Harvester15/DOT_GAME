using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Scenario
{
    public class ScenarioDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public List<string> Objectives;
        public List<string> Evidence;
        public List<string> Witnesses;
        public List<BaseGameObject> Rewards;
        public Node Map;

    }
}
