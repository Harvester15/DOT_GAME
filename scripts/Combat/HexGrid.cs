using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat
{
    /// <summary>
    /// Minimal "offset" hex grid for a HOMM-style battlefield.
    /// Columns x Rows; even-row offset layout. Pixel size is swappable.
    /// </summary>
    public class HexGrid
    {
        public int Columns { get; }
        public int Rows { get; }

        // Visual size of one hex — tweak to match your field art.
        public float HexWidth = 70f;
        public float HexHeight = 60f;
        public Vector2 Origin = new Vector2(80, 80);

        public HexGrid(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public bool InBounds(Vector2I cell) =>
            cell.X >= 0 && cell.X < Columns && cell.Y >= 0 && cell.Y < Rows;

        /// <summary>Hex cell (col,row) to battlefield pixel position (cell center).</summary>
        public Vector2 CellToWorld(Vector2I cell)
        {
            float xOffset = (cell.Y % 2 == 0) ? 0f : HexWidth * 0.5f;
            return Origin + new Vector2(cell.X * HexWidth + xOffset, cell.Y * HexHeight * 0.75f);
        }

        /// <summary>Neighbors for even/odd-row offset hexes.</summary>
        public IEnumerable<Vector2I> Neighbors(Vector2I c)
        {
            bool even = c.Y % 2 == 0;
            var dirs = even
                ? new[] { new Vector2I(-1, 0), new Vector2I(1, 0), new Vector2I(0, -1),
                          new Vector2I(-1, -1), new Vector2I(0, 1), new Vector2I(-1, 1) }
                : new[] { new Vector2I(-1, 0), new Vector2I(1, 0), new Vector2I(1, -1),
                          new Vector2I(0, -1), new Vector2I(1, 1), new Vector2I(0, 1) };

            foreach (var d in dirs)
            {
                var n = c + d;
                if (InBounds(n)) yield return n;
            }
        }

        /// <summary>BFS hex distance (step count) between two cells.</summary>
        public int Distance(Vector2I a, Vector2I b)
        {
            if (a == b) return 0;
            var visited = new HashSet<Vector2I> { a };
            var frontier = new Queue<(Vector2I cell, int dist)>();
            frontier.Enqueue((a, 0));
            while (frontier.Count > 0)
            {
                var (cell, dist) = frontier.Dequeue();
                foreach (var n in Neighbors(cell))
                {
                    if (n == b) return dist + 1;
                    if (visited.Add(n)) frontier.Enqueue((n, dist + 1));
                }
            }
            return int.MaxValue;
        }

        /// <summary>All cells reachable within <paramref name="range"/> steps, ignoring occupancy.</summary>
        public HashSet<Vector2I> CellsInRange(Vector2I start, int range)
        {
            var result = new HashSet<Vector2I>();
            var visited = new HashSet<Vector2I> { start };
            var frontier = new Queue<(Vector2I cell, int dist)>();
            frontier.Enqueue((start, 0));
            while (frontier.Count > 0)
            {
                var (cell, dist) = frontier.Dequeue();
                if (dist >= range) continue;
                foreach (var n in Neighbors(cell))
                {
                    if (visited.Add(n))
                    {
                        result.Add(n);
                        frontier.Enqueue((n, dist + 1));
                    }
                }
            }
            return result;
        }
    }
}
