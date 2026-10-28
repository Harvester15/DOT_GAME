using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Combat.Logic
{
    

    [GlobalClass] // Allows you to create this in the "New Resource" menu
    public partial class ShieldData : Resource
    {
        [Export] public Texture2D Shield;
        [Export] public int MaxShields = 3; // "Health" (strikes to break)
        [Export] public float PhysicalDamageReduction = 0.5f; // Blocks 50% damage while active
        [Export] public float MagicalDamageReduction = 0.5f; // Blocks 50% damage while active
        [Export] public float ElementalDamageReduction = 0.5f; // Blocks 50% damage while active
        [Export] public Damage.DamageType Element; // type of damage blocked 

        // Uses an enum to define what actually damages this specific shield
        [Export] public Godot.Collections.Array<Damage.DamageType> Weaknesses = new Godot.Collections.Array<Damage.DamageType>();
    }


}
