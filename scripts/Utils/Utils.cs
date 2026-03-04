using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Utils
{
    internal static class Utils
    {
        public static Vector2I WorldToGrid(Vector2 worldPosition, int cellSize)
        {
            return new Vector2I(
                Mathf.FloorToInt(worldPosition.X / cellSize),
                Mathf.FloorToInt(worldPosition.Y / cellSize)
            );
        }

        public static Vector2 GridToWorld(Vector2I gridPosition, int cellSize)
        {
            return new Vector2(
                gridPosition.X * cellSize + cellSize / 2,
                gridPosition.Y * cellSize + cellSize / 2
            );
        }

        //get World coordinantes of vector2 arry
        public static Vector2I[] WorldToGrid(Vector2[] gridPositions, int cellSize)
        {
            return gridPositions.Select(pos => WorldToGrid(pos, cellSize)).ToArray();
        }

        public static bool IsValidGridPosition(Vector2I gridPos, int gridWidth, int gridHeight)
        {
            return gridPos.X >= 0 && gridPos.X < gridWidth && gridPos.Y >= 0 && gridPos.Y < gridHeight;
        }

    }
}
