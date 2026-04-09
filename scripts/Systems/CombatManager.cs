
using dot_battle_signal_bus.scripts.Combat;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Array = Godot.Collections.Array;
namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class CombatManager : Node2D
    {
        private List<BaseUnit> playerUnits;
        private List<BaseUnit> enemyUnits;
        private List<BaseUnit> allUnits; // sorted by initiative

        private BaseUnit currentUnit;
        private int currentIndex;
        private CombatResult _combatResult;

        private bool _inCombat = false; // track whether a combat session is active
        private bool _battleReportEndUpdate = false;
        private bool _combatEnded ;

        public override void _Ready()
        {
            SignalBus.Instance.UnitAttacked += OnUnitAttacked;
            SignalBus.Instance.UnitDied += OnUnitDied;
            SignalBus.Instance.UnitKilled += OnUnitKilled;
        }

        private async void OnUnitAttacked(BaseUnit attacker, BaseUnit target)
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
            await StartCombat(player, enemy);
        }

        public async Task StartCombat(List<BaseUnit> player, List<BaseUnit> enemy)
        {
            GD.Print($"Starting combat - Player Units: {string.Join(", ", player.Select(u => u.Name))} vs Enemy Units: {string.Join(", ", enemy.Select(u => u.Name))}");
            SetProcessUnhandledInput(false);//stop player input during combat, could be used to allow some limited input like fleeing or using items
            _combatResult = new CombatResult();
            playerUnits = player;
            enemyUnits = enemy;
            allUnits = new List<BaseUnit>();
            allUnits.AddRange(player);
            allUnits.AddRange(enemy);

            //prepare array for signal
            Array attacker = new Array();
            Array defender = new Array();

            foreach (BaseUnit unit in player)
            {
                attacker.Add(unit);
            }
            foreach (BaseUnit unit in enemy)
            {
                defender.Add(unit);
            }

            // Roll initiative for each unit
            foreach (BaseUnit unit in allUnits)
            {
                unit.Initiative = Dice.RollD10() + unit.Initiative; // we need to store this temp value
            }
            // Sort descending by initiative
            allUnits = allUnits.OrderByDescending(u => u.Initiative).ToList();

            currentIndex = 0;
            _inCombat = true;
            GD.Print($"CombatManager - setting inCombat to true");

            SignalBus.Instance.EmitSignal("CombatStarted", attacker, defender);
            await BeginTurn();
        }

        private async Task BeginTurn()
        {
            if (currentIndex >= allUnits.Count)
                currentIndex = 0; // loop for next round (optional, if multiple rounds)

            currentUnit = allUnits[currentIndex];

            do
            {
                BaseUnit defender = GetDefender(currentUnit);

                if (defender == null)
                {
                    GD.Print($"No valid defender found for {currentUnit.Name}. Skipping turn.");
                    CheckCombatEnded();
                    return;
                }

                await PerformAttack(currentUnit, defender);

                if (currentUnit.CurrentHealth < 1)
                {
                    GD.Print($"Unit {currentUnit.Name} has died in COMBAT.");
                    allUnits.Remove(currentUnit); // remove dead unit from turn order
                    await AdvanceToNextLiving();
                    return;
                }
                await AdvanceToNextLiving();
            } while (enemyUnits.Count > 0);
        }

        private BaseUnit GetDefender(BaseUnit currentUnit)
        {
            if (playerUnits.Contains(currentUnit))
            {
                // Player's turn, target an enemy
                return enemyUnits.Where(u => u.CurrentHealth > 0).FirstOrDefault();
            }
            else
            {
                // Enemy's turn, target a player unit
                return playerUnits.Where(u => u.CurrentHealth > 0).FirstOrDefault();
            }
        }

        private async Task AdvanceToNextLiving()
        {

            currentIndex++;
            if (currentIndex >= allUnits.Count)
                currentIndex = 0;

            if (!CheckCombatEnded())
                await BeginTurn();
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


        private async Task PerformAttack(BaseUnit attacker, BaseUnit defender)
        {
            string attackerName = attacker.Name;
            string defenderName = defender.Name;
            string attackerKey = attacker.Name + "_icon";
            string defenderKey = defender.Name + "_icon";
            //TODO find better solution
            CombatSlot attackerSlot = new CombatSlot();
            attackerSlot.Portrait = attacker.Portrait;
            attackerSlot.UnitTeam = attacker.UnitTeam;
            attackerSlot.UnitName = attacker.Name;

            CombatSlot defenderSlot = new CombatSlot();
            defenderSlot.Portrait = defender.Portrait;
            defenderSlot.UnitTeam = defender.UnitTeam;
            defenderSlot.UnitName = defender.Name;


            GD.Print($"Performing attack - Attacker: {attackerName} (Atk: {attacker.Attack}) vs Defender: {defenderName} (Def: {defender.Defense})");
            int criticalHit = 1;
            bool hit = false;
            string combatLog;

            // To‑hit: d20 + attacker.Attack + modifiers vs defender.Defense + modifiers
            int attackRoll = Dice.RollD20() + attacker.Attack;
            // Apply size penalty: larger defenders are easier to hit
            int defenseValue = defender.Defense; //TODO - (defender.Size * 2) + DefBonus?; // e.g., -2 per size point

            // Other modifiers (terrain, buffs) would be added here

            if (attackRoll >= 20)
            {
                GD.Print($"ataching unit {attackerName} landed a Critical Hit!");
                criticalHit = 2;
                attackRoll += 5; // Example critical hit bonus
                _combatResult.AddCriticalHit(attacker);

                //TODO: add critical hit effects (e.g.,  status effects,
                //perhaps a permanent bonus after x crithits or a new ability - fields/abilities need exp like field/bar?)
            }
            if (attackRoll <= 1)
            {
                attackRoll = 1; // Minimum roll
                criticalHit = 0; // Automatic miss
                //TODO fumble?
            }

            hit = attackRoll >= defenseValue;
            if (hit)
            {

                int damage = Dice.RollDamage(attacker) * criticalHit;
                GD.Print($"sending keys to combatlog {attackerName} - key:{attackerKey}  and defender {defenderName} - key: {defenderKey}");
                combatLog = $"\n{attackerName}  hits {defenderName}  dealing [color=red]{damage}[/color] damage";
                //combatLog = $"\n[img name=\"{attackerKey}\"]  hits [img name=\"{defenderKey}\"]  dealing [color=red]{damage}[/color] damage";
                SignalBus.Instance.EmitSignal("CombatLog", combatLog, attackerSlot, defenderSlot);
                SignalBus.Instance.EmitSignal("CriticalHit", attacker, defender, damage);

                //TODO find a better solution, instead of using a placeholder for the potential killed unit, we should have a separate signal for damage dealt and handle the low health and death logic in the unit scripts, this would also allow us to show damage numbers and have more complex interactions with damage (e.g., armor, resistances, lifesteal, etc.)
                if (defender.CurrentHealth - damage <= 0)
                {
                    GD.Print("in COMBATMANAGER before distributing dmg");
                    OnUnitKilled(attacker, defender);
                }
                defender.TakeDamage(damage);
            }

            await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

            if (defender.CurrentHealth <= 0)
            {

                GD.Print($"Unit {defenderName} has been killed by {attackerName} in COMBAT.");
                combatLog = $"\n{defenderName} has been [color=red]killed[/color] by {attackerName}";

                //combatLog = $"\n[img name=\"{defenderKey}\"] has been [color=red]killed[/color] by [img name=\"{attackerKey}\"]";
                SignalBus.Instance.EmitSignal("CombatLog", combatLog, attackerSlot, defenderSlot);
                if (defender == null)
                {
                    GD.Print($"Unit {defenderName} has been killed by {attackerName} in COMBAT.");
                }
                else if (attacker == null)
                {
                    GD.Print($"Unit {attackerName} has been killed in COMBAT.");
                    SignalBus.Instance.EmitSignal("UnitKilled", attacker, defender);
                }
                await ToSignal(GetTree().CreateTimer(0.6f), "timeout");

            }
            else if ((float)defender.CurrentHealth / (float)defender.MaxHealth <= 0.2)
            {
                //TODO give option of taming if monster, fleeing or captured
                SignalBus.Instance.EmitSignal("UnitLowHealth", defender);
            }
        }

        // Central handler for explicit combat kills (gives killer credit)
        private void OnUnitKilled(BaseUnit killer, BaseUnit victim)
        {
            //if (!_inCombat)
            //    return;

            GD.Print($"CombatManager - UnitKilled handler: {victim.Name} killed by {killer?.Name}");
            if (!_battleReportEndUpdate)
                UpdateBattleReport(killer, victim);

            CheckCombatEnded();
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

        // Generic death handler — handles removals when a unit dies from any cause while combat is active
        private void OnUnitDied(BaseUnit unit)
        {
            if (!_inCombat)
                return;

            GD.Print($"CombatManager - UnitDied handler: {unit.Name} died (non-combat). Removing from turn order.");
            

            //if (!_battleReportEndUpdate)
            //    UpdateBattleReport(killer, victim);

            //CheckCombatEnded();
        }
    }
}
