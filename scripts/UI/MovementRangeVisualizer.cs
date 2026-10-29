using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class MovementRangeVisualizer : Node2D
{
    private int _gridWidth = 50;
    private int _gridHeight = 50;
    [Export] public int CellSize = 32;
    [Export] public Color WalkableColor = Colors.Green;
    [Export] public Color UnwalkableColor = Colors.Red;
    [Export] public Color PathColor = Colors.Yellow;
    [Export] public bool ShowGrid = true;


    //map
    [Export] public Sprite2D MapSprite;
    [Export] public Array<ColorRect> Areas;


    private AStarGrid2D _astarGrid;
    private bool[,] _walkableGrid;
    private Vector2I _gridOffset;
    private List<Vector2I> _currentPath = new List<Vector2I>();
    private BaseUnit _player;

    //movement variables
    public Vector2[][] Movements;
    private bool _isMoving = false;
    private List<Vector2> _movementPath = new List<Vector2>();

    // Ghost units for path visualization
    private List<TextureRect> _pathGhosts = new List<TextureRect>();
    private const float GHOST_SCALE = 0.3f; // Smaller than original unit
    private const float GHOST_ALPHA = 0.5f; // Semi-transparent
    private Color _ghostColor = Colors.Green; // Default ghost color

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
        _gridOffset = Vector2I.Zero; // Adjust if your grid doesn't start at (0,0)
        _ghostColor.A = GHOST_ALPHA; // Set ghost color transparency
        SignalBus.Instance.UnitDeselected += Deselected;
        SignalBus.Instance.NextUnitClicked += Deselected;
    }

    private void Deselected()
    {
        GD.Print("Unit deselected, clearing path ghosts");
        ClearPathGhosts();
    }

    private void SetupGrid()
    {
        _astarGrid = new AStarGrid2D();
        _astarGrid.Region = new Rect2I(new Vector2I(0, 0), new Vector2I(_gridWidth, _gridHeight));
        _astarGrid.CellSize = new Vector2(CellSize, CellSize);
        _astarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Always;
        _astarGrid.Update();

        _walkableGrid = new bool[_gridWidth, _gridHeight];

        // Initialize all as unwalkable
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                _walkableGrid[x, y] = false;
                _astarGrid.SetPointSolid(new Vector2I(x, y), true);
            }
        }
    }

    private void SetupWalkableAreas()
    {
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
                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        _walkableGrid[x, y] = true;
                        _astarGrid.SetPointSolid(new Vector2I(x, y), false);
                    }
                }
            }
        }

        // Remove specific unwalkable spots
        foreach (var spot in _unwalkableSpots)
        {
            if (spot.X >= 0 && spot.X < _gridWidth && spot.Y >= 0 && spot.Y < _gridHeight)
            {
                _walkableGrid[spot.X, spot.Y] = false;
                _astarGrid.SetPointSolid(spot, true);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("grid"))
        {
            GD.Print($"Toggling grid visibility - {ShowGrid}");
            ShowGrid = !ShowGrid; // Toggle grid visibility
            QueueRedraw(); // Trigger _Draw to update grid visibility
        }
    }

    public void DisplayMovementRange(BaseUnit selectedUnit)
    {
        Vector2I startGrid = Utils.WorldToGrid(selectedUnit.GlobalPosition, CellSize);
        Vector2I endGrid = Utils.WorldToGrid(GetGlobalMousePosition(), CellSize);

        // Calculate path between grid positions
        PathPoints = _astarGrid.GetIdPath(startGrid, endGrid).Slice(1);

        // Convert grid coordinates to world coordinates
        var pixelPath = new List<Vector2>();
        foreach (var point in PathPoints)
        {
            Vector2 worldPos = Utils.GridToWorld(point, CellSize);
            pixelPath.Add(worldPos);
        }

        // splitMovement(pixelPath.ToArray());
    }

    public void ShowMovement(Vector2[] newPath, BaseUnit baseUnit)
    {
        GD.Print($"showing movements for {baseUnit}, number of movements: {newPath.Length}");

        _player = baseUnit;
        if (_player == null)
            return;
        ClearPathGhosts();

        List<Vector2> tempListOfPoints = new List<Vector2>();
        Movements = newPath.Chunk(baseUnit.MovementRange).ToArray();

        if (Movements.Length > 0)
        {
            var currentTurnPoints = Movements[0].Chunk(_player.MovementsLeft + 1).ToArray()[0];
            // Current turn's available moves
            if (_player.MovementsLeft > 0)
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
    }

    private void FindPath(Vector2I start, Vector2I end)
    {
        _currentPath.Clear();

        if (!Utils.IsValidGridPosition(start, _gridWidth, _gridHeight) || !Utils.IsValidGridPosition(end, _gridWidth, _gridHeight))
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



    public override void _Draw()
    {
        if (!ShowGrid) return;

        // Draw grid

        foreach (var area in _walkableAreas)
        {
            GD.Print($"Drawing grid...from areas: {area}, {area.Position} with size: {area.Size}");

            for (int x = area.Position.X; x < area.Position.X + area.Size.X; x++)
            {
                for (int y = area.Position.Y; y < area.Position.Y + area.Size.Y; y++)
                {
                    if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
                    {
                        Vector2 worldPos = Utils.GridToWorld(new Vector2I(x, y), CellSize);
                        Rect2 cellRect = new Rect2(worldPos.X - CellSize / 2, worldPos.Y - CellSize / 2,
                        CellSize, CellSize);

                        Color cellColor = _walkableGrid[x, y] ? WalkableColor : UnwalkableColor;
                        cellColor.A = 0.3f; // Make semi-transparent

                        DrawRect(cellRect, cellColor);
                        DrawRect(cellRect, Colors.Black, false, 1); // Border
                    }
                }
            }
        }

        // Draw current path
        if (_currentPath.Count > 1)
        {
            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                Vector2 start = Utils.GridToWorld(_currentPath[i], CellSize);
                Vector2 end = Utils.GridToWorld(_currentPath[i + 1], CellSize);
                DrawLine(start, end, PathColor, 4);
            }
        }
    }

    // Method to get the current path (for player movement)
    public List<Vector2> GetWorldPath()
    {
        var worldPath = new List<Vector2>();
        foreach (var gridPoint in _currentPath)
        {
            worldPath.Add(Utils.GridToWorld(gridPoint, CellSize));
        }
        return worldPath;
    }



    private void CreateGhostUnit(Vector2 position, Color color)
    {
        TextureRect ghost = new TextureRect();
        // Parent ghosts to the world so they follow camera movement/zoom
        var world = GetTree().Root.GetNode<Node2D>("/root/Main/GameWorld");
        world.AddChild(ghost);

        // Copy the unit's sprite properties
        TextureRect unitSprite = _player.Portrait;
        ghost.Texture = unitSprite.Texture;
        ghost.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        ghost.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

        //old sprite2d code
        //ghost.Hframes = unitSprite.Hframes;
        //ghost.Vframes = unitSprite.Vframes;
        //ghost.Frame = unitSprite.Frame;
        //ghost.Scale = unitSprite.Scale * GHOST_SCALE;


        // Position and scale the ghost
        Vector2 originalSize = unitSprite.Size ;
        ghost.Size = originalSize * GHOST_SCALE  ;
        ghost.Scale = new Vector2(GHOST_SCALE,GHOST_SCALE); // Keep the same scale as the original unit
        ghost.GlobalPosition = position;
        GD.Print($"Creating ghost at {position} with size {ghost.Size} and scale {ghost.Scale}");
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
}