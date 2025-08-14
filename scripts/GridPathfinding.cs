using dot_battle_signal_bus.scripts;
using Godot;
using Godot.Collections;
using System;
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
    [Export] public bool ShowGrid = true;


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

    // Ghost units for path visualization
    private List<Sprite2D> _pathGhosts = new List<Sprite2D>();
    private const float GHOST_SCALE = 0.6f; // Smaller than original unit
    private const float GHOST_ALPHA = 0.5f; // Semi-transparent
    private Color _ghostColor = Colors.Green; // Default ghost color

    private GridManager _gridManager => GetNode<GridManager>("/root/Main/GameWorld/Map");

    // Path highlighting
    Array<Vector2I> PathPoints = new Array<Vector2I>();
    public List<Vector2> TargetPath = new List<Vector2>();

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

        GridHeight = MapSprite.Texture.GetHeight();
        GridWidth = MapSprite.Texture.GetWidth();
        _gridOffset = Vector2I.Zero; // Adjust if your grid doesn't start at (0,0)
        _ghostColor.A = GHOST_ALPHA; // Set ghost color transparency
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


    }

    public Vector2[] GetMovementPath(BaseUnit player)
    {
        _player = player; // Set the player reference
        Vector2I startGrid = Utils.WorldToGrid(_player.Position, _gridManager.GetCellSize());
        Vector2I endGrid = Utils.WorldToGrid(GetGlobalMousePosition(),_gridManager.GetCellSize());

        // Calculate path between grid positions
        PathPoints = _gridManager.GetGrid().GetIdPath(startGrid, endGrid).Slice(1);

        // Convert grid coordinates to world coordinates
        var pixelPath = new List<Vector2>();
        foreach (var point in PathPoints)
        {
            Vector2 worldPos = GridToWorld(point);
            pixelPath.Add(worldPos);
        }

        // splitMovement(pixelPath.ToArray());
        return pixelPath.ToArray();
    }

    private void splitMovement(Vector2[] newPath)
    {
        ClearPathGhosts();

        List<Vector2> tempListOfPoints = new List<Vector2>();
        Movements = newPath.Chunk(MovementPoints).ToArray();

        if (Movements.Length > 0)
        {
            var currentTurnPoints = Movements[0].Chunk(MovementsLeft + 1).ToArray()[0];
            // Current turn's available moves
            if (MovementsLeft > 0)
            {
                foreach (var point in currentTurnPoints.Skip(1)) // Skip first point as it's the unit's position
                {
                    CreateGhostUnit(point, Colors.Green);
                }
            }
            // Remaining moves in current turn (if any)
            if (Movements[0].Length > currentTurnPoints.Length)
            {
                var remainingPoints = Movements[0].Skip(currentTurnPoints.Length);
                foreach (var point in remainingPoints)
                {
                    CreateGhostUnit(point, Colors.Red);
                }
            }

            // Handle future turns
            for (int i = 1; i < Movements.Length; i++)
            {
                foreach (var point in Movements[i])
                {
                    CreateGhostUnit(point, Colors.Red);
                }
            }
        }
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_astarGrid == null)
        {
            GD.PrintErr($"GridPathfinding's grid is not set! Please ensure the grid is initialized before using it. setting grid to {_astarGrid}");
            _astarGrid = _gridManager.GetGrid();
        }
        if (_walkableGrid == null)
        {
            GD.PrintErr("Walkable grid is not initialized! Please ensure the grid is set up correctly. in GridPathfinding");
            _walkableGrid = _gridManager.GetWalkableGrid();
        }
        if (Input.IsActionJustPressed("grid"))
        {
            ShowGrid = !ShowGrid; // Toggle grid visibility
            QueueRedraw(); // Trigger _Draw to update grid visibility
        }
        if (Input.IsActionJustPressed("hide"))
        {
            foreach (var area in _areas)
            {
                area.Visible = !area.Visible;
            }
        }
        if (Input.IsActionJustPressed("end"))
        {
            MovementsLeft = 6; // Reset movements left
            //drawMovement(); //redraw/update movement path
        }

        //if (_isMoving)
        //    if (MovementsLeft > 0)
        //    {
        //        // Move player towards the next ghost unit if available
        //        GD.Print($"Moving player towards ghost units, movements left: {MovementsLeft}");
        //        GD.Print($"Moving player towards ghost units, movementpath left: {_movementPath.Count}");

        //        if (_movementPath.Count > 0)
        //        {
        //            SignalBus.Instance.EmitSignal("UnitMoved", _player, _player.Position, _movementPath[0]);

        //            _player.Position = _player.Position.MoveToward(_movementPath[0], 6);
        //            GD.Print("still moving");
        //            if (_player.Position == _movementPath[0])
        //            {
        //                MovementsLeft--;
        //                _movementPath.RemoveAt(0); // Remove the first point after moving
        //                GD.Print($"Moved to next point, movements left: {MovementsLeft}");
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            _isMoving = false;
        //            GD.Print("Movement finished, resetting path ghosts.");
        //        }
        //    }
    }

    private void FindPath(Vector2I start, Vector2I end)
    {
        _currentPath.Clear();

        if (!IsValidGridPosition(start) || !IsValidGridPosition(end))
            return;

        if (!_walkableGrid[start.X, start.Y] || !_walkableGrid[end.X, end.Y])
            return;

        var pathPoints = _astarGrid.GetIdPath(start, end);
        foreach (var point in pathPoints)
        {
            _currentPath.Add(point);
        }

        QueueRedraw(); // Trigger _Draw to update visual path

        // Emit signal or call method to move player along path
        if (_currentPath.Count > 0)
        {
            GD.Print($"Path found with {_currentPath.Count} points");
            // You can emit a signal here to notify the player controller
        }
    }

    private Vector2I WorldToGrid(Vector2 worldPosition)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPosition.X / CellSize),
            Mathf.FloorToInt(worldPosition.Y / CellSize)
        );
    }

    private Vector2 GridToWorld(Vector2I gridPosition)
    {
        return new Vector2(
            (gridPosition.X) * CellSize + CellSize / 2,
            (gridPosition.Y) * CellSize + CellSize / 2
        );
    }

    private bool IsValidGridPosition(Vector2I gridPos)
    {
        return gridPos.X >= 0 && gridPos.X < GridWidth && gridPos.Y >= 0 && gridPos.Y < GridHeight;
    }

    //public override void _Draw()
    //{
    //    if (!ShowGrid) return;

    //    // Draw grid

    //    foreach (var area in _walkableAreas)
    //    {
    //        GD.Print($"Drawing grid...from areas: {area}, {area.Position} with size: {area.Size}");

    //        for (int x = area.Position.X; x < area.Position.X + area.Size.X; x++)
    //        {
    //            for (int y = area.Position.Y; y < area.Position.Y + area.Size.Y; y++)
    //            {
    //                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
    //                {
    //                    Vector2 worldPos = GridToWorld(new Vector2I(x, y));
    //                    Rect2 cellRect = new Rect2(worldPos.X - CellSize / 2, worldPos.Y - CellSize / 2,
    //                    CellSize, CellSize);

    //                    Color cellColor = _walkableGrid[x, y] ? WalkableColor : UnwalkableColor;
    //                    cellColor.A = 0.3f; // Make semi-transparent

    //                    DrawRect(cellRect, cellColor);
    //                    DrawRect(cellRect, Colors.Black, false, 1); // Border
    //                }
    //            }
    //        }
    //    }
    //    //Vector2 worldPos = GridToWorld(new Vector2I(x, y));
    //    //        Rect2 cellRect = new Rect2(
    //    //            worldPos.X - CellSize / 2,
    //    //            worldPos.Y - CellSize / 2,
    //    //            CellSize,
    //    //            CellSize
    //    //        );

    //    //        Color cellColor = _walkableGrid[x, y] ? WalkableColor : UnwalkableColor;
    //    //        cellColor.A = 0.3f; // Make semi-transparent

    //    //        DrawRect(cellRect, cellColor);
    //    //        DrawRect(cellRect, Colors.Black, false, 1); // Border
    //    //    }
    //    // }

    //    // Draw current path
    //    if (_currentPath.Count > 1)
    //    {
    //        for (int i = 0; i < _currentPath.Count - 1; i++)
    //        {
    //            Vector2 start = GridToWorld(_currentPath[i]);
    //            Vector2 end = GridToWorld(_currentPath[i + 1]);
    //            DrawLine(start, end, PathColor, 4);
    //        }
    //    }
    //}

    // Method to get the current path (for player movement)
    public List<Vector2> GetWorldPath()
    {
        var worldPath = new List<Vector2>();
        foreach (var gridPoint in _currentPath)
        {
            worldPath.Add(GridToWorld(gridPoint));
        }
        return worldPath;
    }

    // Method to check if a world position is walkable
    public bool IsWalkable(Vector2 worldPosition)
    {
        Vector2I gridPos = WorldToGrid(worldPosition);
        return IsValidGridPosition(gridPos) && _walkableGrid[gridPos.X, gridPos.Y];
    }


    private void CreateGhostUnit(Vector2 position, Color color)
    {
        Sprite2D ghost = new Sprite2D();
        AddChild(ghost);

        // Copy the unit's sprite properties
        Sprite2D unitSprite = _player.GetSprite();
        ghost.Texture = unitSprite.Texture;
        ghost.Hframes = unitSprite.Hframes;
        ghost.Vframes = unitSprite.Vframes;
        ghost.Frame = unitSprite.Frame;

        // Position and scale the ghost
        ghost.Position = position;
        ghost.Scale = unitSprite.Scale * GHOST_SCALE;

        // Set transparency and color
        color.A = GHOST_ALPHA;
        ghost.Modulate = color;

        // Set Z-index to be below the actual unit
        ghost.ZIndex = _player.ZIndex - 1;

        _pathGhosts.Add(ghost);
    }

    private void ClearPathGhosts()
    {
        foreach (var ghost in _pathGhosts)
        {
            ghost.QueueFree();
        }
        _pathGhosts.Clear();
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
        return GridToWorld(randomPos);
    }

}