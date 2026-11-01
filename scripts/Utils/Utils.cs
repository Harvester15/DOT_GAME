using Godot;
using System;
using System.Linq;

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


        /// <summary>Six corners of a pointy-top hex centered at <paramref name="center"/>.</summary>
        public static Vector2[] HexCorners(Vector2 center, float width, float height)
        {
            var pts = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.DegToRad(60 * i - 90);
                pts[i] = center + new Vector2(Mathf.Cos(a) * width * 0.5f, Mathf.Sin(a) * height * 0.5f);
            }
            return pts;
        }

        public static Vector2[] ClosePolygon(Vector2[] poly)
        {
            var closed = new Vector2[poly.Length + 1];
            poly.CopyTo(closed, 0);
            closed[poly.Length] = poly[0];
            return closed;
        }

    }
}