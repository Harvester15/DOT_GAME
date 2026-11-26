using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public partial class LocationState : Node
    {
        public bool Visited;

        public int VisitCount;
        public bool Discovered;
    }
}
