//using dot_battle_signal_bus.scripts;
//using Godot;
//using System;
//using System.Runtime.CompilerServices;

//public partial class player : Area2D
//{
//    [Export]
//    public Sprite2D _sprite;
//    [Export]
//    public CollisionShape2D _collisionShape;
//    [Export] public int CellSize = 32;

//    private AStarGrid2D _grid;
//    private GridPathfinding _pathfinding;
//    private bool _hasBeenPlaced = false;

//    // Called when the node enters the scene tree for the first time.
//    public override void _Ready()
//    {
//        GD.Print($"Player node {this.Name} is ready.");


//        _grid = GridManager.GetGrid();
//        _pathfinding = GetNode<GridPathfinding>("/root/Map");
//        ScalePlayerToCellSize();
//        PlacePlayerRandomly();

//        SignalBus.Instance.Connect("UnitSelected", new Callable(this, nameof(OnUnitSelected)));
//        SignalBus.Instance.Connect("UnitMoved", new Callable(this, nameof(OnUnitMoved)));
//    }

//    private void ScalePlayerToCellSize()
//    {
//        GD.Print($"Scaling player {this.Name} to CellSize: {CellSize}, current size is {this._sprite.Scale}");
//        // Scale sprite (if texture isn't already CellSize x CellSize)
//        float scaleFactor = (float)CellSize / this._sprite.Texture.GetWidth();
//        this._sprite.Scale = new Vector2(scaleFactor, scaleFactor);
//        GD.Print($"player {this.Name} scaled to size {this._sprite.Scale}");

//        // Resize CircleShape2D radius to half of CellSize (for tight fit)
//        var circleShape = (CircleShape2D)_collisionShape.Shape;
//        circleShape.Radius = CellSize / 2.0f;
//    }

//    private void PlacePlayerRandomly()
//    {
//        if (!_hasBeenPlaced)
//        {
//            GD.Print($"Placing player {this.Name} randomly on the grid, current position {this.Position}.");
//            // Get a random walkable position from the pathfinding system
//            this.Position = _pathfinding.GetRandomWalkablePosition();
//            GD.Print($"Player {this.Name} placed at position {this.Position} on the grid.");
//            _hasBeenPlaced = true;
//        }
//        else if (_grid == null)
//        {
//            GD.PrintErr("Grid is not initialized!");

//        }
//        else if (_pathfinding == null)
//        {
//            GD.PrintErr("Pathfinding system is not initialized!");
//        }

//        //    if (_grid != null)
//        //    {
//        //        Vector2 randomWorldPos = _grid.GetRandomWalkablePosition();
//        //        if (randomWorldPos != Vector2.Zero)
//        //        {
//        //            GlobalPosition = randomWorldPos;
//        //        }
//        //        else
//        //        {
//        //            GD.PrintErr("No walkable positions found!");
//        //        }
//        //    }
//    }
//    // Called every frame. 'delta' is the elapsed time since the previous frame.
//    public override void _Process(double delta)
//    {
//        if (_grid == null)
//            _grid = GridManager.GetGrid();
//        if (_grid != null)
//            PlacePlayerRandomly();
//        // Handle player input or other logic here
//    }

//    public override void _Input(InputEvent @event)
//    {
//        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
//        {

//        }
//    }

//    public Sprite2D GetSprite()
//    {
//        return _sprite;
//    }


//    private void OnUnitSelected(player unit)
//    {
//        GD.Print($"Unit selected: {unit.Name}");
//        // Update UI or other systems
//    }

//    private void OnUnitMoved(player unit, Vector2 from, Vector2 to)
//    {
//        GD.Print($"Unit {unit.Name} moved from {from} to {to} while emitting a signal");
//        // Update UI or other systems
//    }

//    //on input event
//    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
//    {
//        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
//        {
//            GD.Print($"Player {this.Name} clicked from input event ");
//            SignalBus.Instance.EmitSignal("UnitSelected", this);
//        }
//    }
//}
