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
       // [Export] public Texture2D JrpgSprite;
        [Export] public NodePath ShieldIcon;
       
        [Export] public Damage Dmg;
        [Export] public UnitRace UnitRace;

        [Export] public int XpReward;

        [ExportGroup("Base Sprites")]
        [Export] public Texture2D DefaultPortrait;
        [Export] public Texture2D JrpgSprite;

        [ExportGroup("Faction Skin Overrides")]
        [Export] public Texture2D PlayerSkinOverride;
        [Export] public Texture2D EnemySkinOverride;
    }

}
