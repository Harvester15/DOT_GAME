using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dot_battle_signal_bus.scripts.Combat.Systems;
using dot_battle_signal_bus.scripts.Combat.Logic;
using dot_battle_signal_bus.scripts.Combat.Systems;
using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Combat
{


    /// <summary>
    /// Visual + input layer for HOMM combat. Add as "HommField" under
    /// CombatPanel/Homm. Draws hexes/units and turns clicks into actions.
    /// </summary>
    public partial class HommField : Node2D
    {
        // Swappable art — assign in the editor.
        [Export] public Texture2D FieldBackground;
        [Export] public Texture2D HexTileTexture;
        [Export] public Color MoveTint = new Color(0.3f, 0.6f, 1f, 0.5f);
        [Export] public Color AttackTint = new Color(1f, 0.3f, 0.3f, 0.5f);
        [Export] public Color DefaultTint = new Color(1f, 1f, 1f, 0.15f);

        private HommCombatSystem _system;
        private HexGrid _grid;
        private IReadOnlyDictionary<BaseUnit, Vector2I> _positions;

        private BaseUnit _activeUnit;
        private HashSet<Vector2I> _moveCells = new();

        [ExportGroup("Cursors")]
        [Export] public Texture2D AttackCursor;   // res://resources/cursors/attack.png
        [Export] public Texture2D CastCursor;     // res://resources/cursors/cast.png
        [Export] public Texture2D MoveCursor;     // res://resources/cursors/move.png


        // --- add near the top of the class ---
        private enum FieldMode { Normal, Casting }
        private FieldMode _mode = FieldMode.Normal;
        private HashSet<Vector2I> _castCells = new();

        /// <summary>Called by the action bar when the player presses Cast.</summary>
        public void EnterCastMode(BaseUnit caster)
        {
            if (caster.Data == null || !caster.Data.CanCast) return;
            _mode = FieldMode.Casting;
            _castCells = _grid.CellsInRange(_positions[caster], caster.Data.SpellRange);
            _castCells.Add(_positions[caster]); // allow self/ally targeting on own hex if desired
            UpdateCursor();
            QueueRedraw();
        }

        private void ExitCastMode()
        {
            _mode = FieldMode.Normal;
            _castCells.Clear();
            UpdateCursor();
            QueueRedraw();
        }

        /// <summary>Swaps the mouse cursor based on current mode / hovered target.</summary>
        private void UpdateCursor()
        {
            switch (_mode)
            {
                case FieldMode.Casting:
                    SetCursor(CastCursor);
                    break;
                default:
                    SetCursor(null);   // reset to arrow
                    break;
            }
        }

        private void SetCursor(Texture2D tex)
        {
            if (tex != null)
                Input.SetCustomMouseCursor(tex, Input.CursorShape.Arrow, tex.GetSize() / 2);
            else
                Input.SetCustomMouseCursor(null);   // restore default
        }

        public void Initialize(HommCombatSystem system, HexGrid grid,
                               IReadOnlyDictionary<BaseUnit, Vector2I> positions)
        {
            _system = system;
            _grid = grid;
            _positions = positions;
            QueueRedraw();
        }

        public void BeginPlayerTurn(BaseUnit unit)
        {
            _activeUnit = unit;
            _moveCells = _grid.CellsInRange(_positions[unit], unit.MovementRange);
            SetCursor(MoveCursor);   // show move cursor while the unit can act
            QueueRedraw();
        }

        public void Refresh() => QueueRedraw();

        public override void _Draw()
        {
            if (_grid == null) return;

            if (FieldBackground != null)
                DrawTexture(FieldBackground, Vector2.Zero);

            // Hexes — drawn as polygons via shared Utils helpers.
            for (int y = 0; y < _grid.Rows; y++)
                for (int x = 0; x < _grid.Columns; x++)
                {
                    var cell = new Vector2I(x, y);
                    var pos = _grid.CellToWorld(cell);
                    var tint = DefaultTint;
                    if (_activeUnit != null && _moveCells.Contains(cell)) tint = MoveTint;
                    if (_mode == FieldMode.Casting && _castCells.Contains(cell))
                        tint = new Color(0.7f, 0.3f, 1f, 0.5f);   // purple = castable

                    var verts = Utils.Utils.HexCorners(pos, _grid.HexWidth, _grid.HexHeight);
                    DrawColoredPolygon(verts, tint);
                    DrawPolyline(Utils.Utils.ClosePolygon(verts), new Color(1, 1, 1, 0.35f), 1.5f);
                }

            // Units — scaled to fit a cell (not native sprite size).
            foreach (var kv in _positions)
            {
                var unit = kv.Key;
                var pos = _grid.CellToWorld(kv.Value);
                var sprite = unit.Data?.HommSprite;

                if (_activeUnit != null && !_system.IsPlayerSide(unit) &&
     _grid.Distance(_positions[_activeUnit], kv.Value) <= Mathf.Max(1, _activeUnit.AttackRange))
                    DrawColoredPolygon(Utils.Utils.HexCorners(pos, _grid.HexWidth, _grid.HexHeight), AttackTint);

                if (sprite != null)
                {
                    var texSize = sprite.GetSize();
                    float target = Mathf.Min(_grid.HexWidth, _grid.HexHeight) * 0.9f;
                    float scale = target / Mathf.Max(texSize.X, texSize.Y);
                    var drawn = texSize * scale;

                    bool flip = !_system.IsPlayerSide(unit);
                    if (flip)
                    {
                        // Mirror horizontally but keep centered on the cell:
                        // left edge = pos.X + drawn.X/2, width = -drawn.X.
                        var rect = new Rect2(
                            new Vector2(pos.X + drawn.X / 2, pos.Y - drawn.Y / 2),
                            new Vector2(-drawn.X, drawn.Y));
                        DrawTextureRectRegion(sprite, rect, new Rect2(Vector2.Zero, texSize));
                    }
                    else
                    {
                        DrawTextureRect(sprite, new Rect2(pos - drawn / 2, drawn), false);
                    }
                }
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_activeUnit == null) return;

            // Hover: show attack cursor over valid melee/ranged targets (Normal mode only).
            // Hover (Normal mode):
            if (@event is InputEventMouseMotion && _mode == FieldMode.Normal)
            {
                var hover = _grid.WorldToCell(GetLocalMousePosition());   // was WorldToCell(...)
                var hovered = hover.HasValue ? _system.UnitAt(hover.Value) : null;
                bool canAttack = hovered != null && !_system.IsPlayerSide(hovered) &&
                    _grid.Distance(_positions[_activeUnit], hover.Value) <= Mathf.Max(1, _activeUnit.AttackRange);
                SetCursor(canAttack ? AttackCursor : null);
                return;
            }

            if (@event is not InputEventMouseButton mb || !mb.Pressed) return;

            // Right-click cancels casting.
            if (mb.ButtonIndex == MouseButton.Right && _mode == FieldMode.Casting)
            {
                ExitCastMode();
                return;
            }

            if (mb.ButtonIndex != MouseButton.Left) return;

            var cell = _grid.WorldToCell(GetLocalMousePosition());
            GD.Print($"click local={GetLocalMousePosition()} -> cell={cell}, active={_activeUnit?.UnitName}");
            if (cell == null) return;
            var clicked = _system.UnitAt(cell.Value);

            // --- Casting mode: click a unit within spell range to cast on it ---
            if (_mode == FieldMode.Casting)
            {
                if (clicked != null && _castCells.Contains(cell.Value))
                {
                    ExitCastMode();
                    Submit(HommAction.Cast(clicked));
                }
                return;
            }

            // --- Normal mode: attack enemy in reach ---
            if (clicked != null && !_system.IsPlayerSide(clicked) &&
                _grid.Distance(_positions[_activeUnit], cell.Value) <= Mathf.Max(1, _activeUnit.AttackRange))
            {
                SetCursor(null);
                Submit(HommAction.Attack(clicked));
                return;
            }

            // --- Normal mode: move to reachable empty hex ---
            if (clicked == null && _moveCells.Contains(cell.Value))
            {
                Submit(HommAction.Move(cell.Value));
            }
        }

        private void Submit(HommAction action)
        {
            _activeUnit = null;
            _moveCells.Clear();
            SetCursor(null);           // always restore default on action submit
            _system.SubmitPlayerAction(action);
            QueueRedraw();
        }



    }
}
