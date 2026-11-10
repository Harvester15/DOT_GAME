using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    [GlobalClass]
    public partial class WitnessData : Resource
    {
        [Export]
        public string WitnessId { get; set; }

        [Export]
        public string DisplayName { get; set; }

        [Export]
        public Texture2D Portrait { get; set; }

        [Export]
        public Texture2D Background { get; set; }

        [Export(PropertyHint.MultilineText)]
        public string IntroStatement { get; set; }

        [Export]
        public ClueEntry[] KnownClues { get; set; }
    }
}
