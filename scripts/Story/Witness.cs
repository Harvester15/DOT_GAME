using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Story
{
    [GlobalClass]
    public partial class Witness : Resource
    {
        [Export]
        public string Id;

        [Export]
        public string Name;

        [Export]
        public Array<string> KnownEvidence;

        [Export]
        public string ScenarioId;
    }
}
