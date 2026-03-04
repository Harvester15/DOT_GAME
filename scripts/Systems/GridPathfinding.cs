using dot_battle_signal_bus.scripts.Utils;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GridPathfinding : Node2D
{
    [Export] public int GridWidth = 50;
    [Export] public int GridHeight = 50;
    [Export] public int CellSize = 32;
    [Export] public Color WalkableColor = Colors.Green;
    [Export] public Color UnwalkableColor = Colors.Red;
    [Export] public Color PathColor = Colors.Yellow;
    [Export] public bool showGrid = false;


    //map
    [Export] public Sprite2D MapSprite;
    private Array<ColorRect> _areas;


    private AStarGrid2D _astarGrid;
    private bool[,] _walkableGrid;
    private Vector2I _gridOffset;
    private List<Vector2I> _currentPath = new List<Vector2I>();
    private BaseUnit _player;

    //movement variables
    public int MovementPoints = 6;
    public Vector2[][] Movements;
    public int MovementsLeft = 6;
    private bool _isMoving = false;
    private List<Vector2> _movementPath = new List<Vector2>();

    private GridManager _gridManager => GetNode<GridManager>("/root/Main/GameWorld/Map");

    // Path highlighting
    Array<Vector2I> PathPoints = new Array<Vector2I>();
    public List<Vector2> TargetPath = new List<Vector2>();
    private bool showUnits = false;

    // Define walkable areas manually (you'll need to adjust these coordinates based on your map)
    private readonly List<Rect2I> _walkableAreas = new List<Rect2I>
    {
    };

    // Define specific unwalkable spots within walkable areas (like holes, obstacles)
    private readonly List<Vector2I> _unwalkableSpots = new List<Vector2I>
    {
        // Add specific grid coordinates that should be unwalkable
        // Example: new Vector2I(25, 25), // hole in platform
    };

    public override void _Ready()
    {
        _areas = _gridManager.Areas; // Get the areas from GridManager
        GD.Print($"GridPathfinding is ready with areas: {_areas.Count}"); // Debug print to check areas
                                                                          //if (Areas == null)
                                                                          //{
                                                                          //    GD.PrintErr("ColorRect is not assigned! Please assign it in the inspector.");
                                                                          //    return;
                                                                          //}

        //    _player = GetNode<BaseUnit>("/root/Main/GameWorld/Units/BaseUnit"); // Adjust the path to your player node

        GridHeight = _gridManager.GridHeight;
        GridWidth = _gridManager.GridWidth;
        _gridOffset = Vector2I.Zero; // Adjust if your grid doesn't start at (0,0)
                                     //SetupGrid();
                                     // SetupWalkableAreas();
                                     // GridManager.SetGrid(_astarGrid); // Set the grid in GridManager
    }


    //private void SetupGrid()
    //{
    //    _astarGrid = new AStarGrid2D();
    //    _astarGrid.Region = new Rect2I(new Vector2I(0, 0), new Vector2I(GridWidth, GridHeight));
    //    _astarGrid.CellSize = new Vector2(CellSize, CellSize);
    //    _astarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Always;
    //    _astarGrid.Update();

    //    _walkableGrid = new bool[GridWidth, GridHeight];

    //    // Initialize all as unwalkable
    //    for (int x = 0; x < GridWidth; x++)
    //    {
    //        for (int y = 0; y < GridHeight; y++)
    //        {
    //            _walkableGrid[x, y] = false;
    //            _astarGrid.SetPointSolid(new Vector2I(x, y), true);
    //        }
    //    }
    //}

    //private void SetupWalkableAreas()
    //{
    //    foreach (var field in _areas)
    //    {
    //        //add first walkable area from the colorRect
    //        _walkableAreas.Add(new Rect2I(
    //            (int)field.GlobalPosition.X / CellSize, (int)field.GlobalPosition.Y / CellSize,
    //            (int)field.Size.X / CellSize, (int)field.Size.Y / CellSize));
    //    }

    //    // Set walkable areas
    //    foreach (var area in _walkableAreas)
    //    {
    //        for (int x = area.Position.X; x < area.Position.X + area.Size.X; x++)
    //        {
    //            for (int y = area.Position.Y; y < area.Position.Y + area.Size.Y; y++)
    //            {
    //                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
    //                {
    //                    _walkableGrid[x, y] = true;
    //                    _astarGrid.SetPointSolid(new Vector2I(x, y), false);
    //                    GD.Print($"Setting walkable area at ({x}, {y}) to true in gridPathfinding");
    //                }
    //            }
    //        }
    //    }

    //    // Remove specific unwalkable spots
    //    foreach (var spot in _unwalkableSpots)
    //    {
    //        if (spot.X >= 0 && spot.X < GridWidth && spot.Y >= 0 && spot.Y < GridHeight)
    //        {
    //            _walkableGrid[spot.X, spot.Y] = false;
    //            _astarGrid.SetPointSolid(spot, true);
    //        }
    //    }
    //}

    public override void _Input(InputEvent @event)
    {
        //if (@event is InputEventMouseMotion)
        //{
        //    // drawMovement();
        //}

        //if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        //{
        //    Vector2 worldPos = GetGlobalMousePosition();
        //    Vector2I gridPos = WorldToGrid(worldPos);



        //    if (IsValidGridPosition(gridPos) && _walkableGrid[gridPos.X, gridPos.Y])
        //    {
        //        // Find path from current position to clicked position
        //        // Vector2I startPos = WorldToGrid(GetGlobalPosition()); // Assuming this node represents the player
        //        Vector2I startPos = WorldToGrid(GetGlobalMousePosition()); // Assuming this node represents the player
        //        FindPath(startPos, gridPos);
        //    }

        //    if (_currentPath.Count > 0 && !_isMoving)
        //    {
        //        // Move player along the path
        //        _isMoving = true;
        //        _movementPath.Clear();

        //        foreach (var ghost in _pathGhosts)
        //        {
        //            if (ghost.Modulate == _ghostColor)
        //                _movementPath.Add(ghost.Position);
        //        }

        //        // movementsLeft = movementPoints; // Reset movement points for the new path
        //        Movements = new Vector2[1][]; // Initialize movements array
        //        Movements[0] = GetWorldPath().ToArray(); // Store the path in movements
        //                                                 // splitMovement(movements[0]);
        //    }
        //    else
        //    {
        //        GD.Print("No valid path found!");
        //    }
        //}
        if (Input.IsActionJustPressed("grid"))
        {
            showGrid = !showGrid;
            QueueRedraw();
            GD.Print($"GridPathfinding: ShowGrid set to {showGrid}");
        }

        if (Input.IsActionJustPressed("hide"))
        {
            foreach (var area in _areas)
                area.Visible = !area.Visible;
        }
        if (Input.IsActionJustPressed("findUnits"))
        {
            GD.Print($"Toggling unit visibility on grid - {showUnits}");
            showUnits = !showUnits;
            QueueRedraw();
        }


    }

    public Vector2[] GetMovementPath(BaseUnit player)
    {

        //TDOD use camera to get mouse position in world instead of GetGlobalMousePosition
        _player = player; // Set the player reference
        GD.Print($"GridPathfinding: Calculating movement path for player at {_player.GlobalPosition} wile clicked {GetGlobalMousePosition()}, local positin clicked {GetLocalMousePosition()}");

        Vector2I startGrid = Utils.WorldToGrid(_player.Position, _gridManager.GetCellSize());
        Vector2I endGrid = Utils.WorldToGrid(GetLocalMousePosition(), _gridManager.GetCellSize());

        GD.Print($"end point position: {Utils.GridToWorld(endGrid, _gridManager.GetCellSize())}");

        GD.Print($"GridPathfinding: Start grid {startGrid}, End grid {endGrid}");

        // Calculate path between grid positions
        PathPoints = _gridManager.GetGrid().GetIdPath(startGrid, endGrid).Slice(1);

        // Convert grid coordinates to world coordinates
        var pixelPath = new List<Vector2>();
        foreach (var point in PathPoints)
        {
            GD.Print($"Path point: {point}");
            Vector2 worldPos = Utils.GridToWorld(point, _gridManager.GetCellSize());
            pixelPath.Add(worldPos);
        }

        return pixelPath.ToArray();
    }


    public override void _Process(double delta)
    {
        if (_astarGrid == null)
        {
            _astarGrid = _gridManager.GetGrid();
        }
        if (_walkableGrid == null)
        {
            _walkableGrid = _gridManager.GetWalkableGrid();
        }


    }

    public override void _Draw()
    {
        GD.Print("GridPathfinding: _Draw called");
        if (showGrid)
            ShowGrid();

        //show units as points on grid
        if (showUnits)
            ShowUnits();


        //TODO check if this is needed
        //// Draw currently calculated path on top
        //if (_currentPath.Count > 1)
        //{
        //    for (int i = 0; i < _currentPath.Count - 1; i++)
        //    {
        //        var start = Utils.GridToWorld(_currentPath[i], cellSize);
        //        var end = Utils.GridToWorld(_currentPath[i + 1], cellSize);
        //        DrawLine(start, end, PathColor, 4);
        //    }
        //}

        //foreach (var ghost in _pathGhosts)
        //{
        //    DrawTexture(ghost.Texture, ghost.GlobalPosition - (ghost.Texture.GetSize() * ghost.Scale) / 2, ghost.Modulate);
        //}



    }

    // Method to get the current path (for player movement)
    public List<Vector2> GetWorldPath()
    {
        var worldPath = new List<Vector2>();
        foreach (var gridPoint in _currentPath)
        {
            worldPath.Add(Utils.GridToWorld(gridPoint, _gridManager.GetCellSize()));
        }
        return worldPath;
    }

    // Method to check if a world position is walkable
    public bool IsWalkable(Vector2 worldPosition)
    {
        Vector2I gridPos = Utils.WorldToGrid(worldPosition, _gridManager.GetCellSize());
        return Utils.IsValidGridPosition(gridPos, _gridManager.GridWidth, _gridManager.GridHeight) && _walkableGrid[gridPos.X, gridPos.Y];
    }




    ///grid functs
    //return a random walkable position in the grid
    public Vector2 GetRandomWalkablePosition(bool[,] walkableGrid)
    {
        List<Vector2I> walkablePositions = new List<Vector2I>();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (walkableGrid[x, y])
                {
                    walkablePositions.Add(new Vector2I(x, y));
                }
            }
        }

        if (walkablePositions.Count == 0)
            return Vector2.Zero;

        Vector2I randomPos = walkablePositions[GD.RandRange(0, walkablePositions.Count - 1)];
        return Utils.GridToWorld(randomPos, _gridManager.GetCellSize());
    }

    private void ShowUnits()
    {
        float radius = _gridManager.GetCellSize() * 0.28f;

        // Primary color for occupied markers
        Color occColor = Colors.Cyan;
        occColor.A = 0.9f;

        // Outline color (slightly darker, semi-transparent)
        Color outlineColor = Colors.Black;
        outlineColor.A = 0.6f;

        foreach (var cell in _gridManager.GetOccupiedCells())
        {

            GD.Print($"Drawing occupied cell at: {cell.Value}, for {cell.Key}");


            var worldPos = Utils.GridToWorld(cell.Value, _gridManager.GetCellSize());
            // Draw a slightly larger outline circle first to simulate an outline
            DrawCircle(worldPos, radius + 2.0f, outlineColor);
            // Draw the main filled circle
            DrawCircle(worldPos, radius, occColor);
        }
    }

    private void ShowGrid()
    { // Use GridManager as single source of truth
        var cellSize = _gridManager.GetCellSize();
        var walkable = _gridManager.GetWalkableGrid();
        var widthMax = _areas.Max(area => area.GlobalPosition.X + area.Size.X);
        var heightMax = _areas.Max(area => area.GlobalPosition.Y + area.Size.Y);

        var width = _gridManager.GridWidth;
        var height = _gridManager.GridHeight;

        if (walkable == null)
            return;

        // Loop exactly 'width' times and 'height' times
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                // Use local multiplication so it stops exactly at (width * cellSize)
                // Do NOT use a world-space offset here
                Vector2 localPos = new Vector2(x * cellSize, y * cellSize);

                // Define the square for this specific cell
                var rect = new Rect2(localPos, new Vector2(cellSize, cellSize));

                Color cellColor = walkable[x, y] ? WalkableColor : UnwalkableColor;
                cellColor.A = 0.25f;

                DrawRect(rect, cellColor); // Fills the cell
                DrawRect(rect, Colors.Black, false, 1); // Draws the 1px boundary
            }
        }


    }

}