using dot_battle_signal_bus.scripts.Combat.Interface;
using dot_battle_signal_bus.scripts.Combat.Logic;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseGameObject;

namespace dot_battle_signal_bus.scripts.Combat.Systems
{
    public partial class DarkestCombatSystem : BaseCombatSystem
    {
        private CombatResult _combatResult;

        private Node2D darkest;
        private List<Marker2D> attackerPositions = new List<Marker2D>();
        private List<Marker2D> defenderPositions = new List<Marker2D>();
        private Label _currentUnitInfoLabel;
        private Label _unitInfoLabel;
        private List<JrpgUnit> _allUnits;
        private JrpgUnit _selectedUnit;


        private TaskCompletionSource<JrpgUnit> _targetClickedSource;

        public override void _Ready()
        {
            darkest = GetNode<Node2D>("/root/Main/UI/CombatPanel/Darkest");
            darkest.Visible = false;
            _unitInfoLabel = darkest.GetNode<Label>("UnitInfoPanel/HBoxContainer/UnitInfoLabel");
            _currentUnitInfoLabel = darkest.GetNode<Label>("UnitInfoPanel/HBoxContainer/CurrentUnitInfoLabel");
            this.Name = "DarkestCombatSystem";
            GD.Print($"DarkestCombatSystem ready and is visible  - {darkest.Visible}");
            SignalBus.Instance.JrpgUnitSelected += OnJrpgUnitSelected;
            SignalBus.Instance.JrpgUnitClickeded += OnJrpgUnitClicked;
            SignalBus.Instance.UnitKilled += OnUnitKilled;
        }

        private void OnJrpgUnitClicked(JrpgUnit clickedUnit)
        {
            GD.Print($"Unit clicked: {clickedUnit.UnitData.UnitName}");
            _selectedUnit = clickedUnit;
            if (_targetClickedSource == null)
                _targetClickedSource = new TaskCompletionSource<JrpgUnit>();
            _targetClickedSource?.TrySetResult(clickedUnit);
        }

        private void OnJrpgUnitSelected(JrpgUnit SelectedUnit)
        {
            _unitInfoLabel.Text = $"Name: {SelectedUnit.UnitData.Name}\nFaction: {SelectedUnit.UnitData.UnitTeam}\nHP: {SelectedUnit.UnitData.CurrentHealth}/{SelectedUnit.UnitData.MaxHealth}\nAttack: {SelectedUnit.UnitData.Attack}\nDefense: {SelectedUnit.UnitData.Defense}";
        }

        public override async Task<CombatResult> Execute(CombatContext context)
        {
            InitializeUI(context._attacker, context._defender);
            await StartCombat(context._attacker, context._defender);
            return _combatResult;
        }

        private void InitializeUI(List<BaseUnit> attacker, List<BaseUnit> defender)
        {
            darkest.Visible = true;
            var markers = darkest.FindChildren("*", "Marker2D");
            Node parent = new Node();
            _allUnits = new List<JrpgUnit>();
            foreach (var marker in markers)
            {
                parent = marker.GetParent();

                if (marker.Name.ToString().Contains("Attacker"))
                {
                    attackerPositions.Add(marker as Marker2D);
                }
                else if (marker.Name.ToString().Contains("Defender"))
                {
                    defenderPositions.Add(marker as Marker2D);
                }
            }

            for (int i = 0; i < attacker.Count; i++)
            {
                var unit = attacker[i];
                var marker = attackerPositions[i];

                var jrpgScene = GD.Load<PackedScene>("res://scenes/jrpg_unit.tscn");
                var jrpgUnit = jrpgScene.Instantiate<JrpgUnit>();

                jrpgUnit.Position = marker.Position;
                jrpgUnit.UnitSprite.Texture = unit.Data.JrpgSprite;
                jrpgUnit.Scale = new Vector2(0.4f, 0.4f); 
                jrpgUnit.UnitData = unit; 
                jrpgUnit.HealthBar.Value = unit.CurrentHealth;
                
                parent.AddChild(jrpgUnit);
                _allUnits.Add(jrpgUnit);
            }

            for (int i = 0; i < defender.Count; i++)
            {
                var unit = defender[i];
                var marker = defenderPositions[i];

                var jrpgScene = GD.Load<PackedScene>("res://scenes/jrpg_unit.tscn");
                var jrpgUnit = jrpgScene.Instantiate<JrpgUnit>();

                jrpgUnit.Position = marker.Position;
                jrpgUnit.UnitSprite.Texture = unit.Data.JrpgSprite;
                jrpgUnit.UnitSprite.FlipH = true; // Flip the defender sprite horizontally
                jrpgUnit.Scale = new Vector2(0.4f, 0.4f);
                jrpgUnit.UnitData = unit;
                jrpgUnit.HealthBar.Value = unit.CurrentHealth;
                parent.AddChild(jrpgUnit);
                _allUnits.Add(jrpgUnit);
            }
        }

        private async Task StartCombat(List<BaseUnit> attacker, List<BaseUnit> defender)
        {
            GD.Print("Starting combat... Rolling for initiative!");
            foreach (JrpgUnit unit in _allUnits)
            {
                unit.UnitData.Initiative = Dice.RollD10() + unit.UnitData.Initiative; // we need to store this temp value
            }

            _allUnits = _allUnits.OrderByDescending(u => u.UnitData.Initiative).ToList();

            bool combatRunning = true;
            _combatResult = new CombatResult();
            while (combatRunning)
            {
                foreach (var unit in _allUnits)
                {
                    if (unit.UnitData.CurrentHealth <= 0) continue; // Skip dead units
                    

                    _selectedUnit = unit;
                    _currentUnitInfoLabel.Text = $"Current Unit\nName: {unit.UnitData.Name}\nHP: {unit.UnitData.CurrentHealth}/{unit.UnitData.MaxHealth}\nAttack: {unit.UnitData.Attack}\nDefense: {unit.UnitData.Defense}";
                    UpdateCurrentUnitUI(unit);

                    // 2. WAIT for the unit to act
                    // If it's an AI, wait for a timer/logic. 
                    // If it's a player, wait for a signal/TaskCompletionSource.
                    await ExecuteUnitTurn(unit);

                    // 3. Check for Victory/Defeat after every action
                    if (CheckCombatOver())
                    {
                        GD.Print("Combat is over!");
                        combatRunning = false;
                        break;
                    }
                }
            }
            //

        }

        private void UpdateCurrentUnitUI(JrpgUnit unit)
        {
           // throw new NotImplementedException();
        }

        private bool CheckCombatOver()
        {
            _selectedUnit = null;
            return _allUnits.All(u => u.UnitData.UnitTeam == Team.Player || u.UnitData.CurrentHealth <= 0) || // All enemies dead
                   _allUnits.All(u => u.UnitData.UnitTeam == Team.Enemy || u.UnitData.CurrentHealth <= 0);   // All players dead
                                                                                                             //  throw new NotImplementedException();
        }

        private async Task ExecuteUnitTurn(JrpgUnit unit)
        {
            // Example: Add a slight delay so the player can see who is acting
            await Task.Delay(1000);

            if (unit.UnitData.UnitTeam == Team.Player)
            {
                GD.Print("Waiting for player to click a target...");

                // 1. Prepare the "waiter"
                _targetClickedSource = new TaskCompletionSource<JrpgUnit>();
                GD.Print("waiter is ready");

                // 2. Wait here until OnUnitClicked is triggered
                JrpgUnit target = await _targetClickedSource.Task;
                GD.Print("target selected");

                // 3. Clear the waiter and perform the attack
                _targetClickedSource = null;
                await PerformAttack(unit, target);
            }
            else
            {
                // Simple AI: Pick a random Hero
                var target = _allUnits.Where(u => u.UnitData.UnitTeam == Team.Player).First();
                await PerformAttack(_selectedUnit, target);
            }

        }
        private async Task PerformAttack(JrpgUnit attacker, JrpgUnit defender)
        {
            GD.Print($"{attacker.UnitData.Name} attacks {defender.UnitData.Name}");
            //1. Calculate attack result (hit/miss/crit/damage)
            var result = CombatRules.CalculateAttack(attacker.UnitData, defender.UnitData);

            // 2. Start both tasks (Don't await them individually yet!)

            Task attackAnim = attacker.PlayAttackAnimation(defender.GlobalPosition);
            Task reactionAnim = Task.CompletedTask;
            if (result.IsHit)
            {
                reactionAnim = defender.PlayHitReaction();
                defender.HealthBar.Value = defender.UnitData.CurrentHealth - result.Damage;
                if (defender.HealthBar.Value / defender.UnitData.MaxHealth < 0.25)
                {
                    var newStyle = new StyleBoxFlat();
                    newStyle.BgColor = Colors.Red;
                    defender.HealthBar.AddThemeStyleboxOverride("fill", newStyle);
                }
                else if (defender.HealthBar.Value / defender.UnitData.MaxHealth < 0.5)
                {
                    var newStyle = new StyleBoxFlat();
                    newStyle.BgColor = Colors.Orange;
                    defender.HealthBar.AddThemeStyleboxOverride("fill", newStyle);
                }
                    // Visual Popups

                    if (result.IsCritical)
                    defender.SpawnPopUp("CRITICAL!!", Colors.Orange);

                defender.SpawnPopUp($"-{result.Damage}", Colors.Red);
            }
            else
            {
                defender.SpawnPopUp("MISS", Colors.Gray);
            }

            await Task.WhenAll(attackAnim, reactionAnim);

            // 3. Apply Damage and check for death
            if (result.IsHit)
            {
                defender.UnitData.TakeDamage(result.Damage);

                if (defender.UnitData.CurrentHealth <= 0)
                {
                    await defender.PlayDeathAnimation();
                    UpdateBattleReport(attacker.UnitData, defender.UnitData);
                    _allUnits.Remove(defender); // Remove from the initiative loop
                }
            }
        }


        private void OnUnitKilled(BaseUnit killer, BaseUnit victim)
        {
            //if (!_inCombat)
            //    return;

            GD.Print($"Darkest Combat System - UnitKilled handler: {victim.Name} killed by {killer?.Name}");

            //TODO remove the bool, doesn't do anything?!!?
                UpdateBattleReport(killer, victim);

            CheckCombatOver();
        }

        private void UpdateBattleReport(JrpgUnit killer, JrpgUnit victim)
        {
            GD.Print($"updating combat result: {victim.Name}  - {victim.UnitData.ExperienceReward} XP");
            _combatResult.AddKill(killer.UnitData);
            _combatResult.AddXPEarned(killer.UnitData, victim.UnitData.Data.XpReward);
            _combatResult.TotalXPPool += victim.UnitData.ExperienceReward;

            _combatResult.AddLoss(victim.UnitData);
            _combatResult.UnitDied(victim.UnitData);
            //if (_playerUnits != null && _playerUnits.Contains(victim))
            //    _playerUnits.Remove(victim);
            //if (_enemyUnits != null && _enemyUnits.Contains(victim))
            //    _enemyUnits.Remove(victim);
            if (_allUnits != null && _allUnits.Contains(victim))
                _allUnits.Remove(victim);
            victim.QueueFree(); // Remove the unit from the scene
        }

    }
}
