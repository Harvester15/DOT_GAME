using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    [GlobalClass]
    public partial class LocationData : Resource
    {
        [Export]
        public string Id;

        [Export]
        public string Name;

        [Export(PropertyHint.MultilineText)]
        public string Description;

        [Export]
        public Texture2D Icon;

        [Export]
        public Texture2D Background;

        [Export]
        public WitnessData Witness;

    }
}
