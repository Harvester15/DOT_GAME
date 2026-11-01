using dot_battle_signal_bus.scripts;
using Godot;

namespace dot_battle_signal_bus.scripts.Homm
{
    /// <summary>Picked up for score/gating. POC: gold is a gate, not an economy.</summary>
    public partial class GoldPile : MapObject
    {
        [Export] public int Amount = 500;
        public override void OnVisited(ArmyStack v) { v.AddResource("gold", Amount); QueueFree(); }
    }

    /// <summary>Spend gold for extra units before the battle (risk/reward).</summary>
    public partial class MercenaryCamp : MapObject
    {
        [Export] public int Cost = 500;
        public override void OnVisited(ArmyStack v)
        {
            v.CarriedResources.TryGetValue("gold", out var g);
            if (g < Cost) return;
            v.CarriedResources["gold"] = g - Cost;
            // TODO: add a mercenary UnitData to v.Units
        }
    }

    /// <summary>The point of the whole map: surfaces a clue (doc/relic/note).</summary>
    public partial class EvidenceSite : MapObject
    {
        [Export] public Investigation.ClueEntry Clue;
        public override void OnVisited(ArmyStack v)
        {
            if (Clue == null) return;
            // DialogueRunner/Logbook decide Mark vs Read — nothing auto-logs.
            SignalBus.Instance.EmitSignal(
                SignalBus.SignalName.ClueEncountered, Clue.ClueId, Clue.DisplayText);
        }
    }

    /// <summary>Ends the scenario → triggers the testimony battle → back to Archive.</summary>
    public partial class ScenarioExit : MapObject
    {
        [Export] public Investigation.BattleEcho Testimony;
        public override void OnVisited(ArmyStack v) =>
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.EchoRequested,
                Testimony.EventId, Testimony.WitnessId);
    }
}