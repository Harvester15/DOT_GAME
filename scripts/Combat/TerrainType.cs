using Godot;

namespace dot_battle_signal_bus.scripts.Combat
{
    /// <summary>
    /// Where a battle is claimed to take place. In the Archive of Echoes the Terrain
    /// can DIFFER between witnesses — a markable contradiction.
    /// </summary>
    [GlobalClass]
    public partial class TerrainType : Resource
    {
        [Export] public string TerrainId;     // "array_site", "temple", "beast_pen"
        [Export] public string DisplayName;
        [Export] public float AttackerModifier = 1.0f;
        [Export] public float DefenderModifier = 1.0f;
    }
}