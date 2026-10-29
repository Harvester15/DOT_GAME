
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Combat.Interface;
using dot_battle_signal_bus.scripts.Combat.Systems;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Array = Godot.Collections.Array;
namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class CombatManager : Node2D
    {
        public ICombatSystem combatSystem; // Interface for combat logic, allows for different implementations (e.g., turn-based, real-time, etc.)

        public void SetCombatSystem(ICombatSystem system)
        {
            GD.Print($"CombatManager - Setting combat system to: {system.GetType().Name}");
            combatSystem = system;
        }

        private List<BaseUnit> playerUnits;
        private List<BaseUnit> enemyUnits;
        private List<BaseUnit> allUnits; // sorted by initiative

        private BaseUnit currentUnit;
        private int currentIndex;
        private CombatResult _combatResult;

        private bool _inCombat = false; // track whether a combat session is active
        private bool _battleReportEndUpdate = false;
        private bool _combatEnded ;

        public async Task StartCombat(List<BaseUnit> player, List<BaseUnit> enemy)
        {
            var context = new CombatContext(player, enemy);


            if (combatSystem == null)
            {
                GD.Print("No combat system set");
                return;
            }

            GameManager.Instance.EnterCombat(context);

            var result = await combatSystem.Execute(context);

            ApplyResult(result);
        }

        private void ApplyResult(CombatResult result)
        {
            SignalBus.Instance.EmitSignal("CombatEnded", result);
            GameManager.Instance.ExitCombat();
        }

        public override void _Ready()
        {
            SignalBus.Instance.CombatModeSelected += OnUnitAttacked;
            //SignalBus.Instance.UnitAttacked += OnUnitAttacked;
            //.Instance.UnitDied += OnUnitDied;
            //SignalBus.Instance.UnitKilled += OnUnitKilled;
        }

        private async void OnUnitAttacked(BaseUnit attacker, BaseUnit target, GodotObject system)
        {
            _combatEnded = false;
            GD.Print($"COMBATMANAGER - Combat initiated between {attacker.Name} and {target.Name}");
            List<BaseUnit> player = new List<BaseUnit>();
            if (attacker is ArmyStack)
                player = ((ArmyStack)attacker).Units;
            else
                player.Add(attacker);
            List<BaseUnit> enemy = new List<BaseUnit>();
            if (target is ArmyStack)
                enemy = ((ArmyStack)target).Units;
            else
                enemy.Add(target);

            if(combatSystem == null)
            {
                GD.Print("CombatManager - No combat system set, using default WarlordsCombatSystem");

                if (system is ICombatSystem combatSystem)
                {
                    GD.Print("CombatManager - Combat system provided by signal: " + combatSystem.GetType().Name);
                }
            }
            await StartCombat(player, enemy);
        }

        

        private bool CheckCombatEnded()
        {
            if (_combatEnded)
                return true;

            GD.Print("Checking if combat has ended...");
            bool playerAlive = playerUnits.Any(u => u.CurrentHealth > 0);
            bool enemyAlive = enemyUnits.Any(u => u.CurrentHealth > 0);

            if (!playerAlive || !enemyAlive)
            {
                GD.Print("Combat has ended.");
                // Load the separate UI scene
                //var reportScene = GD.Load<PackedScene>("res://scenes/UI/battle_report.tscn");
                //var reportInstance = reportScene.Instantiate<BattleReport>();

                // Add it to the tree (it will appear on top because it's a CanvasLayer)
                //GetTree().Root.AddChild(reportInstance);

                _inCombat = false;
                GD.Print($"CombatManager - setting inCombat to false");

                SignalBus.Instance.EmitSignal("CombatEnded", _combatResult);
                SetProcessUnhandledInput(true);
                Visible = false;
                _combatEnded = true;
                return true;
            }
            return false;
        }


        

        private void UpdateBattleReport(BaseUnit killer, BaseUnit victim)
        {
            _combatResult.AddKill(killer);
            _combatResult.AddXPEarned(killer, victim.ExperienceReward);
            _combatResult.TotalXPPool += victim.ExperienceReward;
            _combatResult.AddLoss(victim);
            _combatResult.UnitDied(victim);

            if (playerUnits != null && playerUnits.Contains(victim))
                playerUnits.Remove(victim);
            if (enemyUnits != null && enemyUnits.Contains(victim))
                enemyUnits.Remove(victim);
            if (allUnits != null && allUnits.Contains(victim))
                allUnits.Remove(victim);

        }

        //// Generic death handler — handles removals when a unit dies from any cause while combat is active
        //private void OnUnitDied(BaseUnit unit)
        //{
        //    if (!_inCombat)
        //        return;

        //    GD.Print($"CombatManager - UnitDied handler: {unit.Name} died (non-combat). Removing from turn order.");


        //    //if (!_battleReportEndUpdate)
        //    //    UpdateBattleReport(killer, victim);

        //    //CheckCombatEnded();
        //}
    }
}
