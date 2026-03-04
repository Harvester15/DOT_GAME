using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using Godot.Collections;
using System;

public partial class GameManager : Node2D
{
    //map
    [Export] public Array<Area2D> Objects;

    private GridManager _gridManager;
    private AStarGrid2D _grid;
    private bool _hasBeenPlaced;
    private GridPathfinding _pathfinding;
    

    public override void _Ready()
    {
        // Initialize the game state, load resources, etc.
        GD.Print("GameManager is ready.");

        _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
        _grid = _gridManager.GetGrid();
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
        

        // Connect signals or set up initial game state
        foreach (var area in Objects)
        {
            GD.Print($"dealing with gameObject {area.Name}");
            ScalePlayerToCellSize(area);
            PlacePlayerRandomly(area);
        }
        //set camera start position to center of map
        GD.Print("Setting Camera to center of map.");
        CameraManager.Instance.MainCamera.GlobalPosition = new Vector2(_gridManager.GridWidth / 2, _gridManager.GridHeight / 2);
    }

    private void ScalePlayerToCellSize(Area2D area)
    {
      GD.Print($"area {area.Name} is of type  {area.GetType()}");

        BaseGameObject gameObject = new BaseGameObject();
        

        //check if area is null 
        if (area == null)
        {
            GD.PrintErr("Area2D is null, cannot scale player.");
            return;
        }
        // check for which type and cast it as such
        if (area is BaseUnit)
        {
            var player = area as BaseUnit;
            gameObject.Portrait = player.Portrait;
            gameObject.CollisionShape = player.CollisionShape;
        }
        else if (area is Chest)
        {
            var chest = area as Chest;
            gameObject.Portrait = chest.Portrait;
            gameObject.CollisionShape = chest.CollisionShape;
        }
        else
        {
            GD.PrintErr($"Area2D {area.Name} is not a BaseUnit or Player, cannot scale.");
            return;
        }
        gameObject.Portrait = gameObject.GetSprite();

        GD.Print($"Scaling player {gameObject.Name} to CellSize: {_gridManager.GetCellSize()}, current size is {gameObject.Portrait.Scale}");
        // Scale sprite (if texture isn't already CellSize x CellSize)
        float scaleFactor = (float)_gridManager.GetCellSize() / gameObject.Portrait.Texture.GetWidth();
        gameObject.Portrait.Scale = new Vector2(scaleFactor, scaleFactor);
        GD.Print($"player {this.Name} scaled to size {gameObject.Portrait.Scale}");

        // Resize CircleShape2D radius to half of CellSize (for tight fit)
        //var circleShape = (CircleShape2D)gameObject.CollisionShape.Shape;
        //circleShape.Radius = _gridManager.GetCellSize() / 2.0f;
    }

    private void PlacePlayerRandomly(Area2D area)
    {
        BaseGameObject gameObject = new BaseGameObject();


        //check if area is null 
        if (area == null)
        {
            GD.PrintErr("Area2D is null, cannot scale player.");
            return;
        }
        // check for which type and cast it as such
        if (area is BaseUnit)
        {
            var player = area as BaseUnit;
            gameObject.Portrait = player.Portrait;
        }
        else if (area is Chest)
        {
            var chest = area as Chest;
            gameObject.Portrait = chest.Portrait;
        }
        else
        {
            GD.PrintErr($"Area2D {area.Name} is not a BaseUnit or Player, cannot scale.");
            return;
        }
        if (!_hasBeenPlaced)
        {
            GD.Print($"Placing gameObject {gameObject.Name} randomly on the grid, current position {gameObject.Position}.");
            // Get a random walkable position from the pathfinding system
            this.Position = _pathfinding.GetRandomWalkablePosition(_gridManager.GetWalkableGrid());
            if (this.Position == Vector2.Zero)
            {
                GD.PrintErr("No valid walkable position found!");
                return;
            }
            GD.Print($"gameObject {gameObject.Name} placed at position {gameObject.Position} on the grid.");
            _hasBeenPlaced = true;
            _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(gameObject.Position, _gridManager.GetCellSize()), gameObject);
        }
       
    }

    public override void _Input(InputEvent @event)
    {
        //if (Input.IsActionJustPressed("initialize"))
        //{
        //    foreach (var area in Objects)
        //    {
        //        GD.Print($"dealing with gameObject {area.Name}");
        //        ScalePlayerToCellSize(area);
        //        PlacePlayerRandomly(area);
        //    }
        //}
    }
}
