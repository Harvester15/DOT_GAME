using Godot;

namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class FameManager : Node2D
    {
        public int GlobalFame = 0;

        public float GetDiscountModifier() => Mathf.Clamp(GlobalFame / 1000f, 0, 0.5f); // Up to 50% off

        //TODO mangee unit fame as well, for quest tracking and stuff
        public void AddFame(int amount)
        {
            GlobalFame += amount;
            SignalBus.Instance.EmitSignal("FameChanged", GlobalFame);
        }
    }

}
