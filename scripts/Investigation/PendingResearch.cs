using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public partial class PendingResearch : Node
    {
        public string Topic;
        public int TurnsRemaining;
        public string ResultText;
    }
}
