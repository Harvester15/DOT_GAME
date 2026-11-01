using Godot;

namespace dot_battle_signal_bus.scripts.Homm
{
    /// <summary>Recruit one unit type. Refugee→Swordsmen, Monastery→Clerics, Crypt→Skeletons.</summary>
    public partial class Dwelling : MapObject
    {
        [Export] public Actors.Units.UnitData UnitType;
        [Export] public int Available = 5;

        public override void OnVisited(ArmyStack v)
        {
            if (Available <= 0 || UnitType == null) return;
            // TODO: spawn a BaseUnit from UnitType, v.AddUnit(...)
            Available--;
        }
    }
}