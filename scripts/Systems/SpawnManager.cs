using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using Godot.Collections;

public partial class SpawnManager : Node2D
{
    //map
    [Export] public Array<Node2D> GameObjects;

    private GridManager _gridManager;
    private AStarGrid2D _grid;
    private bool _hasBeenPlaced;
    private GridPathfinding _pathfinding;


    public override void _Ready()
    {
        // Initialize the game state, load resources, etc.
        GD.Print("SpawnManager is ready.");
        SignalBus.Instance.Connect("WorldInitialized", new Callable(this, nameof(OnWorldInitialized)));

        _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
        _grid = _gridManager.GetGrid();
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");

        
        //set camera start position to center of map
        GD.Print("Setting Camera to center of map.");
        CameraManager.Instance.MainCamera.GlobalPosition = new Vector2(_gridManager.GridWidth / 2, _gridManager.GridHeight / 2);
    }

    private void OnWorldInitialized()
    {
        if (GameObjects == null || GameObjects.Count == 0)
        {
            GD.PrintErr("No game objects assigned to SpawnManager!");
            return;
        }

        GD.Print($"spawnmanager getting objects to spawn, number of nodes: {GameObjects.Count}");
        // Connect signals or set up initial game state
        foreach (var node in GameObjects)
        {
            var children = node.GetChildren();
            GD.Print($"dealing with node {node.Name}, number of children: {children.Count}");
            foreach (var area in children)
            {
                GD.Print($"dealing with gameObject {area.Name}");
                ScalePlayerToCellSize((BaseGameObject)area);
                PlacePlayerRandomly((BaseGameObject)area);
            }

        }
    }


    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("spawn"))
        {
            OnWorldInitialized();
        }
    }

    private void ScalePlayerToCellSize(Area2D area)
    {
        GD.Print($"area {area.Name} is of type  {area.GetType()}");

        BaseGameObject gameObject;


        //check if area is null 
        if (area == null)
        {
            GD.PrintErr("Area2D is null, cannot scale.");
            return;
        }

        GD.Print($"scaling area {area.Name} to cell size { _gridManager.GetCellSize()}");

        gameObject = area as BaseGameObject;

        gameObject.Portrait = gameObject.GetNodeOrNull<TextureRect>("TextureRect");

        GD.Print($"portrait size before scaling: {gameObject.Portrait.Size}, scale: {gameObject.Portrait.Scale}");

        // 1. Setup the TextureRect for scaling (do this once)
        gameObject.Portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        gameObject.Portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

        gameObject.Portrait.PivotOffset = gameObject.Portrait.Texture.GetSize() / 2;

        float scaleFactor = (float)_gridManager.GetCellSize() / gameObject.Portrait.Texture.GetWidth();

        gameObject.Portrait.Scale = new Vector2(scaleFactor, scaleFactor); // Reset scale to ensure CustomMinimumSize is used
        GD.Print($"Scaled portrait for {gameObject.Name} to fit cell size {_gridManager.GetCellSize()}, current scale is {gameObject.Portrait.Scale}, and size {gameObject.Portrait.Size}");

        gameObject.CollisionShape = gameObject.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        // Access the actual shape resource (assuming it's a CircleShape2D)
        GD.Print($"gameObject {gameObject.Name} has collision shape {gameObject.CollisionShape.Shape.GetType()}");
        if (gameObject.CollisionShape.Shape is CircleShape2D circle)
        {
            GD.Print($"Scaling collision shape for {gameObject.Name} to fit cell size {_gridManager.GetCellSize()}");
            // Make it unique so we don't scale every object using this resource
            CircleShape2D uniqueShape = (CircleShape2D)circle.Duplicate();
            gameObject.CollisionShape.Shape = uniqueShape;

            // Set radius to half the cell size
            uniqueShape.Radius = _gridManager.GetCellSize() / 2.0f;
        }
        GD.Print($"Finished scaling area {area.Name} to cell size {_gridManager.GetCellSize()}");   


        ////// check for which type and cast it as such
        ////if (area is BaseUnit)
        ////{
        ////    var player = area as BaseUnit;
        ////    gameObject.Portrait = player.Portrait;
        ////    gameObject.CollisionShape = player.CollisionShape;
        ////}
        ////else if (area is Chest)
        ////{
        ////    var chest = area as Chest;
        ////    gameObject.Portrait = chest.Portrait;
        ////    gameObject.CollisionShape = chest.CollisionShape;
        ////}
        ////else
        ////{
        ////    GD.PrintErr($"Area2D {area.Name} is not a BaseUnit or Player, cannot scale.");
        ////    return;
        ////}
        ////gameObject.Portrait = gameObject.GetSprite();

        ////GD.Print($"Scaling player {gameObject.Name} to CellSize: {_gridManager.GetCellSize()}, current size is {gameObject.Portrait.Scale} in game manager");
        ////// Scale sprite (if texture isn't already CellSize x CellSize)
        ////float scaleFactor = (float)_gridManager.GetCellSize() / gameObject.Portrait.Texture.GetWidth();
        ////gameObject.Portrait.Scale = new Vector2(scaleFactor, scaleFactor);
        ////GD.Print($"player {this.Name} scaled to size {gameObject.Portrait.Scale} in game manager");

        // Resize CircleShape2D radius to half of CellSize (for tight fit)
        //var circleShape = (CircleShape2D)gameObject.CollisionShape.Shape;
        //circleShape.Radius = _gridManager.GetCellSize() / 2.0f;
    }

    private void PlacePlayerRandomly(Area2D area)
    {
        BaseGameObject gameObject;

        GD.Print($"placing area {area.Name}");
        //check if area is null 
        if (area == null)
        {
            GD.PrintErr("Area2D is null, cannot scale player.");
            return;
        }

        gameObject = area as BaseGameObject;
        // check for which type and cast it as such

        if (!gameObject._hasBeenPlaced)
        {
            GD.Print($"Placing gameObject {gameObject.Name} randomly on the grid, current position {gameObject.Position}. in spawn manager");
            // Get a random walkable position from the pathfinding system
            gameObject.Position = _pathfinding.GetRandomWalkablePosition(_gridManager.GetWalkableGrid());
            if (gameObject.Position == Vector2.Zero)
            {
                GD.PrintErr("No valid walkable position found!");
                return;
            }
            if (_gridManager.IsCellOccupied(gameObject.Position) != null)
                return;
            GD.Print($"gameObject {gameObject.Name} placed at position {gameObject.Position} on the grid.");
            _hasBeenPlaced = true;
            _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(gameObject.Position, _gridManager.GetCellSize()), gameObject);
        }

    }
}
