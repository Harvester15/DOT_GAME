using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat
{
    [GlobalClass]
    public partial class Damage : Resource
    {
        public enum DamageType
        {
            Physical,
            Magical,
            Fire,
            Ice,
            Lightning,
            Poison,
            // Add more types as needed
        }
        // Damage dice: e.g., 2d6+1 → Count=2, Sides=6, Modifier=1
        [Export] public int DamageCount { get; set; } = 1;
        [Export] public int DamageSides { get; set; } = 6;
        [Export] public int DamageModifier { get; set; } = 0;
        //TODO make this an enum or something more robust
        [Export] public DamageType dmgType{ get; set; } = DamageType.Physical; // e.g., Physical, Magical, Fire, etc.
    }
}
