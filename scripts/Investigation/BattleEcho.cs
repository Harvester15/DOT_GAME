using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Actors.Units;
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Systems;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// One witness's playable memory of a shared event, authorable in the Inspector.
    /// The SAME EventId played through different witnesses diverges because the
    /// biased CombatSettings/Terrain/sides differ.
    /// </summary>
    [GlobalClass]
    public partial class BattleEcho : Resource
    {
        [Export] public string WitnessId;
        [Export] public string EventId;                 // shared across witnesses
        [Export] public Godot.Collections.Array<UnitData> MySide = new();   // attacker roster
        [Export] public Godot.Collections.Array<UnitData> TheirSide = new();// defender roster
        [Export] public TerrainType Terrain;            // where THIS witness claims it happened
        [Export] public CombatSettings Settings;        // bias: buffs, inflated counts, "miracles"
        [Export] public bool Replayable = true;
    }

    public static class EchoRunner
    {
        /// <summary>Replays a witness's biased version through the existing combat engine.</summary>
        public static async Task Play(CombatManager mgr, List<BaseUnit> attacker, List<BaseUnit> defender, BattleEcho echo)
        {
            if (!echo.Replayable) return;

            SignalBus.Instance.EmitSignal(
                SignalBus.SignalName.EchoRequested, echo.EventId, echo.WitnessId);

            // CombatManager builds the CombatContext; ICombatSystem stays untouched.
            await mgr.StartCombat(attacker, defender);
        }
    }
}