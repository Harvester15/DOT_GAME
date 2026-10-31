using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.UI
{
        /// <summary>
        /// Route A minimap: draws the map texture scaled to fit, then overlays
        /// unit dots and the current camera view rectangle. Click to pan the camera.
        /// </summary>
        public partial class MiniMap : Control
        {
            // Assigned in the editor so paths aren't hard-coded here.
            [Export] public GridManager Grid;
            [Export] public Camera2D Camera;

            [Export] public float DotRadius = 2.5f;
            [Export] public Color PlayerColor = Colors.LimeGreen;
            [Export] public Color EnemyColor = Colors.Red;
            [Export] public Color NeutralColor = Colors.Gray;
            [Export] public Color ViewRectColor = new Color(1, 1, 1, 0.6f);

            private Texture2D _mapTexture;
            private Vector2 _mapWorldSize;   // full map size in world pixels

            public override void _Ready()
            {
                if (Grid?.MapSprite?.Texture == null)
                {
                    GD.PrintErr("MiniMap: GridManager/MapSprite texture not set.");
                    return;
                }

                _mapTexture = Grid.MapSprite.Texture;
                _mapWorldSize = _mapTexture.GetSize();

            GD.Print($"grid and camera assigned to minimap: {Grid}, {Camera}, with {Grid.MapSprite}");
            }

            public override void _Process(double delta)
            {
                // Units and camera move every frame, so keep the overlay fresh.
                QueueRedraw();
            }

        public override void _Draw()
        {
            var tex = Grid?.MapSprite?.Texture;
            if (tex == null) return;

            _mapWorldSize = tex.GetSize();
            var scale = MapToMiniScale();

            var drawnSize = _mapWorldSize * scale;
            var origin = (Size - drawnSize) / 2f;   // centers the thumbnail in the control

            // 1. Map, centered.
            DrawTextureRect(tex, new Rect2(origin, drawnSize), false);

            // 2. Unit dots (origin-shifted + validity-guarded — see Issue 2).
            foreach (var pair in Grid.GetOccupiedCells())
            {
                if (pair.Key is not BaseUnit unit) continue;
                if (!GodotObject.IsInstanceValid(unit)) continue;   // skip freed units

                var dotPos = origin + unit.GlobalPosition * scale;
                DrawCircle(dotPos, DotRadius, TeamColor(unit.UnitTeam));
            }

            // 3. Camera view rectangle.
            if (Camera != null)
            {
                var viewWorld = GetViewportRect().Size / Camera.Zoom;
                var topLeft = origin + (Camera.GlobalPosition - viewWorld / 2f) * scale;
                DrawRect(new Rect2(topLeft, viewWorld * scale), ViewRectColor, false, 1f);
            }
        }

        // --- Click to pan the camera ---
        public override void _GuiInput(InputEvent @event)
            {
                if (@event is InputEventMouseButton mb && mb.Pressed &&
                    mb.ButtonIndex == MouseButton.Left && Camera != null)
                {
                    // Minimap-local click -> world position (inverse of the draw scale).
                    var worldPos = mb.Position / MapToMiniScale();
                    Camera.GlobalPosition = worldPos;
                    AcceptEvent();
                }
            }

            /// <summary>Uniform scale that fits the whole map inside this control, preserving aspect.</summary>
            private Vector2 MapToMiniScale()
            {
                var s = Mathf.Min(Size.X / _mapWorldSize.X, Size.Y / _mapWorldSize.Y);
            return new Vector2(s, s);
            }

            private Color TeamColor(BaseUnit.Team team) => team switch
            {
                BaseUnit.Team.Player => PlayerColor,
                BaseUnit.Team.Enemy => EnemyColor,
                _ => NeutralColor
            };
        }
    }
