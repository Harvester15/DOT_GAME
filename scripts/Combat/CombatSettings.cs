using Godot;

namespace dot_battle_signal_bus.scripts.Combat
{
    /// <summary>
    /// Per-battle tuning. For the Archive of Echoes these fields encode a witness's BIAS.
    /// Defaults are neutral so normal combat is unaffected.
    /// </summary>
    [GlobalClass]
    public partial class CombatSettings : Resource
    {
        [Export] public float AttackerDamageMultiplier = 1.0f;
        [Export] public float DefenderDamageMultiplier = 1.0f;
        [Export] public int AttackerInitiativeBonus = 0;
        [Export] public int DefenderInitiativeBonus = 0;

        [Export] public bool ForcedMiracle = false;   // a unit cannot die (Priest's "miracle")
        [Export] public bool InflateCounts = false;   // duplicate side units (over-remembering)
        [Export] public int InflateBy = 0;

        public string WitnessId;   // set at runtime from the echo
        public string EventId;
    }
}