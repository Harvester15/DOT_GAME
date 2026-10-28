using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Actors.Units
{
    [GlobalClass]
    public partial class UnitRace : Resource
    {
        [Export] public string RaceName;
        [Export] public Texture2D RaceIcon;

        [ExportGroup("Immunities & Resistances")]
        [Export] public bool ImmuneToMindControl = false;
        [Export] public bool ImmuneToPoison = false;
        [Export] public float MagicResistance = 0.0f; // 0.1 = 10% reduction

        [ExportGroup("Special Rules")]
        [Export] public bool IsLiving = true; // False for Undead/Constructs
        [Export] public int MoraleBonus = 0;

        [ExportGroup("Stat Bonuses")]
        [Export] public int BonusInitiative = 0;
        [Export] public int BonusAttack = 0;
        [Export] public float DamageMultiplier = 1.0f;

    }

}
