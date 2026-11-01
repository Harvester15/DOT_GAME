using Godot;

namespace dot_battle_signal_bus.scripts.Actors.Units
{
    /// <summary>
    /// A hero commands an ArmyStack. Kept minimal for the POC but gives future HOMM
    /// systems (artifacts, skills, army buffs) a stable home instead of raw units.
    /// The hero is the unit shown "on top" of its stack when present.
    /// </summary>
    public partial class Hero : BaseUnit
    {
        [Export] public int Morale = 0;
        [Export] public int Luck = 0;

        // Reserved for the full game; unused by the POC but present so saves/combat
        // referencing a Hero won't need reshaping later.
        [Export] public Godot.Collections.Array<string> ArtifactIds = new();
        [Export] public int CommandRange = 6; // how far the stack can move per turn
    }
}