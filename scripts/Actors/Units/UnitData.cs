using dot_battle_signal_bus.scripts.Combat;
using Godot;

namespace dot_battle_signal_bus.scripts.Actors.Units
{

    /// <summary>
    /// All the data that defines a unit, such as stats, name, portrait, etc. This is a Resource so it can be easily created and edited in the Godot editor, and then assigned to units in the game.
    /// </summary>

    [GlobalClass]
    public partial class UnitData : Resource
    {
        [Export] public string UnitName;
        [Export] public Texture2D Portrait;
        [Export] public Texture2D AttackIcon;
       
        [Export] public Damage Dmg;
    }

}
