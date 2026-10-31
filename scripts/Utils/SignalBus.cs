// SignalBus.cs
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Combat.Logic;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts
{
    // Singleton pattern for global signal bus
    public partial class SignalBus : Node
    {
        private static SignalBus _instance;

        public static SignalBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SignalBus();
                }
                return _instance;
            }
        }


        // Game signals
        [Signal] public delegate void UnitSelectedEventHandler(BaseUnit unit);
        [Signal] public delegate void UnitDeselectedEventHandler();
        //[Signal] public delegate void UnitMovedEventHandler(BaseUnit unit, Vector2 from, Vector2 to);//is this needed?
        [Signal] public delegate void UnitMovedEventHandler(BaseUnit unit, int movementsLeft);
        [Signal] public delegate void UnitAttackedEventHandler(BaseUnit attacker, BaseUnit target);
        [Signal] public delegate void UnitDefendEventHandler(BaseUnit attacker, BaseUnit target);
        [Signal] public delegate void UnitDiedEventHandler(BaseUnit unit);
        [Signal] public delegate void UnitLowHealthEventHandler(BaseUnit unit);
        [Signal] public delegate void UnitHealthChangedEventHandler(BaseUnit unit);
        // [Signal] public delegate void AbilityUsedEventHandler(Ability ability, player caster, player target);
        [Signal] public delegate void UnitsGroupedEventHandler(ArmyStack armyStack);
        [Signal] public delegate void NextUnitClickedEventHandler();

        //combat signals
        [Signal] public delegate void CombatResolvedEventHandler(CombatResult result);
        [Signal] public delegate void ShieldHitEventHandler(BaseUnit defender, BaseUnit attacker, ShieldData shieldData);
        // UI signals
        [Signal] public delegate void ShowUnitInfoEventHandler(BaseUnit unit);
        [Signal] public delegate void UnitEnteredAllyEventHandler(BaseUnit unitEntered, BaseUnit enteringUnit);
        [Signal] public delegate void UnitExitedAllyEventHandler(BaseUnit unitExited, BaseUnit exitingUnit);
        [Signal] public delegate void HideUnitInfoEventHandler();
        [Signal] public delegate void ShowTileInfoEventHandler(Vector2 position);
        [Signal] public delegate void HideTileInfoEventHandler();

        [Signal] public delegate void CombatModeSelectedEventHandler(BaseUnit attacker, BaseUnit target, GodotObject system);

        [Signal] public delegate void MapPointClickedEventHandler(Vector2 position);

        //turn manager is an autoload, not managed by signalbus
        ////Turn management signals
        //[Signal] public delegate void StateChangedEventHandler(TurnManager.GameState newPhase);
        //[Signal] public delegate void TurnChangedEventHandler(int turnNumber);

        //world events
        [Signal] public delegate void WorldInitializedEventHandler();


        //combat
        [Signal] public delegate void CombatStartedEventHandler(Array<BaseUnit> player, Array<BaseUnit> enemy);
        [Signal] public delegate void CombatEndedEventHandler(CombatResult combatResult);
        [Signal] public delegate void ShowCombatReportEventHandler(CombatResult combatResult);
        [Signal] public delegate void CombatLogEventHandler(string combatReport, CombatSlot attacker, CombatSlot defender);
        [Signal] public delegate void CriticalHitEventHandler(BaseUnit attacker, BaseUnit target, int damage);
        [Signal] public delegate void UnitKilledEventHandler(BaseUnit killer, BaseUnit victim);
        [Signal] public delegate void BattleReportClosedEventHandler();
        //Homm
        [Signal] public delegate void HommAwaitingActionEventHandler(BaseUnit unit);
        // (CombatStarted may already exist — reuse if so.)


        //jrpg combat signals
        [Signal] public delegate void JrpgUnitSelectedEventHandler(JrpgUnit unit);
        [Signal] public delegate void JrpgUnitClickededEventHandler(JrpgUnit unit);
        [Signal] public delegate void CombatTurnStartedEventHandler(BaseUnit currentUnit);
        [Signal] public delegate void CombatTurnEndedEventHandler(BaseUnit currentUnit);
        [Signal] public delegate void CombatActionEventHandler(BaseUnit actor, string actionDescription);
        [Signal] public delegate void CombatDamageEventHandler(BaseUnit attacker, BaseUnit target, int damage);


        // To register the singleton in Godot's autoload:
        public override void _Ready()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                QueueFree(); // Ensure only one instance exists
            }
        }
    }
}
