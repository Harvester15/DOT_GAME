using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;

public partial class Chest : BaseGameObject
{
    [Export] public int GoldAmount = 100;
    [Export] public string ItemName = "Health Potion";
    public Sprite2D Portrait { get; set; }
    public CollisionShape2D CollisionShape;
    private GridManager _gridManager;
    private AStarGrid2D _grid;
    private GridPathfinding _pathfinding;
    private bool _hasBeenPlaced = false;
    private Sprite2D _portrait;

    public override void _Ready()
    {
        base._Ready();
        GD.Print($"Chest ready with {GoldAmount} gold and item: {ItemName}");
        _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
        _grid = _gridManager.GetGrid();
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
        _portrait = Portrait ?? new Sprite2D();
        ScalePlayerToCellSize();
        PlacePlayerRandomly();
    }

    private void ScalePlayerToCellSize()
    {
        GD.Print($"Scaling player {this.Name} to CellSize: {_gridManager.GetCellSize()}, current size is {this._portrait.Scale}");
        // Scale sprite (if texture isn't already CellSize x CellSize)
        float scaleFactor = (float)_gridManager.GetCellSize() / this._portrait.Texture.GetWidth();
        this._portrait.Scale = new Vector2(scaleFactor, scaleFactor);
        GD.Print($"player {this.Name} scaled to size {this._portrait.Scale}");

        // Resize CircleShape2D radius to half of CellSize (for tight fit)
        //var circleShape = (CircleShape2D)CollisionShape.Shape;
        //circleShape.Radius = _gridManager.GetCellSize() / 2.0f;
    }

    private void PlacePlayerRandomly()
    {
        if (!_hasBeenPlaced)
        {
            GD.Print($"Placing player {this.Name} randomly on the grid, current position {this.Position}.");
            // Get a random walkable position from the pathfinding system
            this.Position = _pathfinding.GetRandomWalkablePosition(_gridManager.GetWalkableGrid());
            if (this.Position == Vector2.Zero)
            {
                GD.PrintErr("No valid walkable position found!");
                return;
            }
            GD.Print($"Player {this.Name} placed at position {this.Position} on the grid.");
            _hasBeenPlaced = true;
            _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(this.Position, _gridManager.GetCellSize()), this);
        }
        else if (_grid == null)
        {
            GD.PrintErr("Grid is not initialized!");

        }
        else if (_pathfinding == null)
        {
            GD.PrintErr("Pathfinding system is not initialized!");
        }

    }

    public void OpenChest()
    {
        GD.Print($"You opened the chest and found {GoldAmount} gold and a {ItemName}!");
        // Logic to give player the gold and item
        // Emit signal or call method to update player's inventory
    }

    public override void _Input(InputEvent @event)
    {
        //temp reset of movements left for testing purposes
        if (Input.IsActionJustPressed("initialize"))
        {
            ScalePlayerToCellSize();
            PlacePlayerRandomly();
        }

    }
}
