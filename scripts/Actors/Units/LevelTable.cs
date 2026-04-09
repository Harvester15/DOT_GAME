using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Actors.Units
{
    [GlobalClass]
    public partial class LevelTable : Resource
    {
        [Export] public int[] XPRequirements = { 0, 100, 300, 600, 1000 };
    }

}
