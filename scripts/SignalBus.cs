// SignalBus.cs
using Godot;
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
        [Signal] public delegate void UnitDeselectedEventHandler(BaseUnit unit);
        //[Signal] public delegate void UnitMovedEventHandler(BaseUnit unit, Vector2 from, Vector2 to);//is this needed?
        [Signal] public delegate void UnitMovedEventHandler(BaseUnit unit, int movementsLeft);
        [Signal] public delegate void UnitAttackedEventHandler(BaseUnit attacker, BaseUnit target);
        [Signal] public delegate void UnitDefendEventHandler(BaseUnit attacker, BaseUnit target);
        [Signal] public delegate void UnitDiedEventHandler(BaseUnit unit);
       // [Signal] public delegate void AbilityUsedEventHandler(Ability ability, player caster, player target);

        // UI signals
        [Signal] public delegate void ShowUnitInfoEventHandler(BaseUnit unit);
        [Signal] public delegate void HideUnitInfoEventHandler();
        [Signal] public delegate void ShowTileInfoEventHandler(Vector2 position);
        [Signal] public delegate void HideTileInfoEventHandler();

        //Turn management signals
        [Signal] public delegate void TurnPhaseChangedEventHandler(TurnManager.TurnPhase newPhase);



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
