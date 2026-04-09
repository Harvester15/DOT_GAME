using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace dot_battle_signal_bus.scripts.Utils
{
    public partial class GridManager : Node
    {
        // public static GridManager Instance { get; private set; }
        private AStarGrid2D _astarGrid;

        [Export] public int GridWidth = 50;
        [Export] public int GridHeight = 50;
        [Export] public static int CellSize = 32;

        //map
        [Export] public Sprite2D MapSprite;
        [Export] public Array<ColorRect> Areas;


        private bool[,] _walkableGrid;
        private readonly List<Rect2I> _walkableAreas = new List<Rect2I>
        {
        };

        public System.Collections.Generic.Dictionary<Node2D,  Vector2I> OccupiedCells = new System.Collections.Generic.Dictionary<Node2D, Vector2I>();

        public void SetGrid(AStarGrid2D grid)
        {
            _astarGrid = grid;
        }

        public AStarGrid2D GetGrid()
        {
            if (_astarGrid != null)
                return _astarGrid;
            return null;
        }

        public override void _Ready()
        {

            
            GridHeight = MapSprite.Texture.GetHeight()/CellSize;
            GridWidth = MapSprite.Texture.GetWidth()/CellSize;
            GD.Print($"map texture is {MapSprite}, with dimensions: ({GridHeight}, {GridWidth})");
            SetupGrid();
            SetupWalkableAreas();
        }

        private void SetupGrid()
        {
            GD.Print($"Setting up grid with width: {GridWidth}, height: {GridHeight}, cell size: {CellSize}");
            _astarGrid = new AStarGrid2D();
            _astarGrid.Region = new Rect2I(new Vector2I(0, 0), new Vector2I(GridWidth, GridHeight));
            _astarGrid.CellSize = new Vector2(CellSize, CellSize);
            _astarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Always;
            _astarGrid.Update();

            _walkableGrid = new bool[GridWidth, GridHeight];

            // Initialize all as unwalkable
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    _walkableGrid[x, y] = false;
                    _astarGrid.SetPointSolid(new Vector2I(x, y), true);
                }
            }
        }

        private void SetupWalkableAreas()
        {
            if (Areas == null || Areas.Count == 0)
            {
                GD.PrintErr("No walkable areas defined in the map.");
                return;
            }
            foreach (var field in Areas)
            {
                //add first walkable area from the colorRect
                _walkableAreas.Add(new Rect2I(
                    (int)field.GlobalPosition.X / CellSize, (int)field.GlobalPosition.Y / CellSize,
                    (int)field.Size.X / CellSize, (int)field.Size.Y / CellSize));
            }

            // Set walkable areas
            foreach (var area in _walkableAreas)
            {
                for (int x = area.Position.X; x < area.Position.X + area.Size.X; x++)
                {
                    for (int y = area.Position.Y; y < area.Position.Y + area.Size.Y; y++)
                    {
                        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                        {
                            _walkableGrid[x, y] = true;
                            _astarGrid.SetPointSolid(new Vector2I(x, y), false);
                            //GD.Print($"Setting walkable area at ({x}, {y}) to true in gridmanager");
                        }
                    }
                }
            }

            //// Remove specific unwalkable spots
            //foreach (var spot in _unwalkableSpots)
            //{
            //    if (spot.X >= 0 && spot.X < GridWidth && spot.Y >= 0 && spot.Y < GridHeight)
            //    {
            //        _walkableGrid[spot.X, spot.Y] = false;
            //        _astarGrid.SetPointSolid(spot, true);
            //    }
            //}
        }

        public bool UnitFaction(Vector2 gridPosition, BaseUnit.Team team)
        {
            var gridPositionI = Utils.WorldToGrid(gridPosition, CellSize);
            if (OccupiedCells.ContainsValue(gridPositionI))
            {
                var thing = OccupiedCells.FirstOrDefault(_ => _.Value == gridPositionI) ;
                BaseUnit unit  = thing.Key as BaseUnit;
                if (unit != null)
                {
                    return unit.UnitTeam == team;
                }
                //return OccupiedCells.Where(_ => _.Value == gridPositionI).Select(_ => _.Key) as BaseUnit;
            }
            return false;

        }

        public  BaseUnit IsCellOccupied(Vector2 gridPosition)
        {
            var gridPositionI = Utils.WorldToGrid(gridPosition, CellSize);
            if (OccupiedCells.ContainsValue(gridPositionI))
            {
                //TODO : this is really inefficient, we should have a reverse lookup dictionary for grid position to unit?
                //return BaseGameObject instead of BaseUnit
                var unit = OccupiedCells.FirstOrDefault(_ => _.Value == gridPositionI);
                return unit.Key as BaseUnit;
                //return OccupiedCells.Where(_ => _.Value == gridPositionI).Select(_ => _.Key) as BaseUnit;
            }
            return null;

        }

        //use BaseUnit instead of Node2D
        public  void RegisterUnitOnCell(Vector2I gridPosition, Area2D unit)
        {
            GD.Print($"Registering unit {unit.Name} on cell {gridPosition}");
            if (OccupiedCells.ContainsKey(unit))
            {
                GD.PrintErr($"Unit {unit.Name} is already registered on cell {OccupiedCells[unit]}");
                return;
            }
            OccupiedCells[unit] = gridPosition;
        }

        public  void UnregisterUnitFromCell(BaseUnit unit)
        {
            GD.Print($"Unregistering {unit.UnitName} from cell {OccupiedCells[unit]}");
            OccupiedCells.Remove(unit);
        }




        public int GetCellSize()
        {
            return CellSize;
        }

        public bool[,] GetWalkableGrid()
        {
            return _walkableGrid;
        }

        //get a list of all units and which cells they occupy
        public System.Collections.Generic.Dictionary<Node2D, Vector2I> GetOccupiedCells()
        {
            return OccupiedCells;
        }

    }
}
