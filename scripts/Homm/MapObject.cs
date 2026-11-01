using Godot;

namespace dot_battle_signal_bus.scripts.Homm
{
    /// <summary>Base for anything a stack can visit on the HoMM-Lite map.</summary>
    public abstract partial class MapObject : Area2D
    {
        [Export] public string ObjectId;
        public abstract void OnVisited(ArmyStack visitor);
    }
}