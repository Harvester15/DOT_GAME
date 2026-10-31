using dot_battle_signal_bus.scripts.Combat.Interface;
using dot_battle_signal_bus.scripts.Combat.Logic;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat.Systems
{
    /// <summary>
    /// HOMM-style battle on a hex field. Visuals live under CombatPanel/Homm,
    /// mirroring DarkestCombatSystem — no separate scene file needed.
    /// </summary>
    public partial class HommCombatSystem : BaseCombatSystem
    {
        [ExportGroup("Grid")]
        [Export] public int Columns = 11;
        [Export] public int Rows = 7;

        private Node2D _homm;                 // visual root under CombatPanel (like `darkest`)
        private HommField _field;             // the hex renderer/input node
        private HexGrid _grid;
        private readonly System.Collections.Generic.Dictionary<BaseUnit, Vector2I> _positions = new();
        private List<BaseUnit> _turnOrder = new();
        private TaskCompletionSource<HommAction> _pendingAction;

        private readonly HashSet<BaseUnit> _defending = new();


        public override void _Ready()
        {
            _homm = GetNode<Node2D>("/root/Main/UI/CombatPanel/Homm");
            _homm.Visible = false;
            _field = _homm.GetNode<HommField>("HommField");
            this.Name = "HommCombatSystem";
        }

        public override async Task<CombatResult> Execute(CombatContext context)
        {
            _combatResult = new CombatResult();
            _grid = new HexGrid(Columns, Rows);

            DeployArmies(context._attacker, context._defender);
            RollInitiative();

            // Hand the field its data and show it.
            _homm.Visible = true;
            _field.Initialize(this, _grid, _positions);

            // Reuse the existing typed CombatStarted signal.
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.CombatStarted,
                ToArray(context._attacker), ToArray(context._defender));

            await RunBattleLoop();

            PopulateResult(context);
            _homm.Visible = false;
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.CombatEnded, _combatResult);
            return _combatResult;
        }

        private void DeployArmies(List<BaseUnit> player, List<BaseUnit> enemy)
        {
            for (int i = 0; i < player.Count && i < Rows; i++)
                Place(player[i], new Vector2I(0, i));
            for (int i = 0; i < enemy.Count && i < Rows; i++)
                Place(enemy[i], new Vector2I(Columns - 1, i));
        }

        private void Place(BaseUnit unit, Vector2I cell)
        {
            _positions[unit] = cell;
            unit.GridPosition = cell;
        }

        private void RollInitiative()
        {
            foreach (var u in _positions.Keys.ToList())
                u.Initiative = Dice.RollD10() + u.Initiative;
            _turnOrder = _positions.Keys
                .Where(u => u.CurrentHealth > 0)
                .OrderByDescending(u => u.Initiative).ToList();
        }

        private async Task RunBattleLoop()
        {
            while (!IsBattleOver())
            {
                foreach (var unit in _turnOrder.ToList())
                {
                    if (unit.CurrentHealth <= 0 || IsBattleOver()) continue;
                    SignalBus.Instance.EmitSignal(SignalBus.SignalName.CombatTurnStarted, unit);
                    await TakeTurn(unit);
                    SignalBus.Instance.EmitSignal(SignalBus.SignalName.CombatTurnEnded, unit);
                }
            }
        }

        private async Task TakeTurn(BaseUnit unit)
        {
            _defending.Remove(unit);   // defense bonus lasts only until this unit acts again

            HommAction action = IsPlayerSide(unit)
                ? await RequestPlayerAction(unit)
                : DecideEnemyAction(unit);

            switch (action.Type)
            {
                case HommActionType.Move: MoveUnit(unit, action.TargetCell); break;
                case HommActionType.Attack: if (action.TargetUnit != null) Attack(unit, action.TargetUnit); break;
                case HommActionType.Cast: GD.Print($"{unit.UnitName} casts (TODO: ability system)."); break;
                case HommActionType.Defend: _defending.Add(unit); break;   // grant the bonus
                case HommActionType.Wait:
                default: break;
            }
            _field.Refresh();
        }

        private void MoveUnit(BaseUnit unit, Vector2I target)
        {
            if (!_grid.InBounds(target) || IsOccupied(target)) return;
            if (_grid.Distance(_positions[unit], target) > unit.MovementRange) return;
            Place(unit, target);
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.UnitMoved, unit, unit.MovementsLeft);
        }

        // Apply the bonus in Attack():
        private void Attack(BaseUnit attacker, BaseUnit defender)
        {
            int reach = Mathf.Max(1, attacker.AttackRange);
            if (_grid.Distance(_positions[attacker], _positions[defender]) > reach) return;

            int effectiveDefense = defender.Defense + (_defending.Contains(defender) ? 5 : 0);  // defend bonus
            int damage = Dice.RollDamage(attacker) + attacker.Attack;
            int mitigated = Mathf.Max(1, damage - effectiveDefense);
            defender.TakeDamage(mitigated);
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.CombatDamage, attacker, defender, mitigated);

            if (defender.CurrentHealth <= 0)
            {
                UpdateBattleReport(attacker, defender);
                _combatResult.AddLoss(defender);
                _turnOrder.Remove(defender);
                _positions.Remove(defender);
            }
        }

        private HommAction DecideEnemyAction(BaseUnit unit)
        {
            var target = _positions.Keys
                .Where(u => IsPlayerSide(u) && u.CurrentHealth > 0)
                .OrderBy(u => _grid.Distance(_positions[unit], _positions[u]))
                .FirstOrDefault();
            if (target == null) return HommAction.Wait();

            if (_grid.Distance(_positions[unit], _positions[target]) <= Mathf.Max(1, unit.AttackRange))
                return HommAction.Attack(target);

            var step = _grid.Neighbors(_positions[unit])
                .Where(c => !IsOccupied(c))
                .OrderBy(c => _grid.Distance(c, _positions[target]))
                .DefaultIfEmpty(_positions[unit]).First();
            return HommAction.Move(step);
        }

        // --- UI handshake ---
        private Task<HommAction> RequestPlayerAction(BaseUnit unit)
        {
            _pendingAction = new TaskCompletionSource<HommAction>();
            _field.BeginPlayerTurn(unit);   // field lights up move/attack hexes
            return _pendingAction.Task;
        }

        public void SubmitPlayerAction(HommAction action) => _pendingAction?.TrySetResult(action);

        // --- Helpers ---
        public HexGrid Grid => _grid;
        public IReadOnlyDictionary<BaseUnit, Vector2I> Positions => _positions;
        public bool IsPlayerSide(BaseUnit u) => u.UnitTeam == BaseUnit.Team.Player;
        public bool IsOccupied(Vector2I cell) => _positions.ContainsValue(cell);
        public BaseUnit UnitAt(Vector2I cell) =>
            _positions.FirstOrDefault(kv => kv.Value == cell).Key;

        private bool IsBattleOver()
        {
            bool p = _positions.Keys.Any(u => IsPlayerSide(u) && u.CurrentHealth > 0);
            bool e = _positions.Keys.Any(u => !IsPlayerSide(u) && u.CurrentHealth > 0);
            return !p || !e;
        }

        private void PopulateResult(CombatContext ctx)
        {
            bool playerWon = _positions.Keys.Any(u => IsPlayerSide(u) && u.CurrentHealth > 0);
            _combatResult.Winners = (playerWon ? ctx._attacker : ctx._defender)
                .Where(u => u.CurrentHealth > 0).ToList();
        }

        private static Array<BaseUnit> ToArray(List<BaseUnit> units)
        {
            var arr = new Array<BaseUnit>();
            foreach (var u in units) arr.Add(u);
            return arr;
        }
    }
}
