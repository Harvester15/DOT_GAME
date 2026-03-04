// BaseUnit.cs
using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;

//namespace dot_battle_signal_bus.scripts;

public partial class BaseUnit : Area2D
{

    public enum Team { Neutral, Player, Enemy, Ally }

    [Export] public string UnitName { get; set; } = "Unit";
    [Export] public Team UnitTeam { get; set; } = 0; // 0=neutral, 1=player, 2=enemy, etc.
    public Sprite2D Portrait;// => GetNode<Sprite2D>("/root/Main/GameWorld/Units/BaseUnit/Sprite2D");
    public CollisionShape2D CollisionShape;
    [Export] public int AttackRange = 1;


    [ExportGroup("Team Sprites")]
    [Export] public Texture2D NeutralSprite;
    [Export] public Texture2D PlayerSprite;
    [Export] public Texture2D EnemySprite;
    [Export] public Texture2D AllySprite;

    // Stats
    [ExportGroup("Combat Stats")]
    [Export] public int MaxHealth = 100;
    [Export] public int CurrentHealth { get; set; }
    [Export] public int Attack = 10;
    [Export] public int Defense = 5;
    [Export] public int MovementRange = 5;


    // State
    public bool HasMoved { get; set; } = false;
    public bool HasActed { get; set; } = false;
    public Vector2 GridPosition { get; set; }

    private GridManager _gridManager;
    private AStarGrid2D _grid;
    private GridPathfinding _pathfinding;
    private bool _hasBeenPlaced = false;
    public int MovementsLeft;

    // Components
    private UnitMovement _movement;
    private bool _isSelected = false;

    private Area2D _attackRangeDetector;
    private ProgressBar _healthBar;
    

    //  private UnitVisuals _visuals;
    //   private List<UnitAbility> _abilities = new List<UnitAbility>();

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        MovementsLeft = MovementRange;
        Portrait = GetNode<Sprite2D>("Sprite2D");
        UpdateTeamSprite();
        _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
        _grid = _gridManager.GetGrid();
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
        _healthBar = GetNode<ProgressBar>($"{this.GetPath()}/Sprite2D/HealthBar");
        CollisionShape = GetNode<CollisionShape2D>($"{this.GetPath()}/CollisionShape2D");
        ScalePlayerToCellSize();
        UpdateTeamSprite();
        PlacePlayerRandomly();

        SignalBus.Instance.Connect("UnitMoved", new Callable(this, nameof(OnUnitMoved)));
        SignalBus.Instance.Connect("UnitDeselected", new Callable(this, nameof(OnUnitDeselected)));
        // connect the area entered signal   to the UnitAttack signal
        //connect mouse entered signal to the _OnMouseEntered method
        SignalBus.Instance.Connect("UnitAttacked", new Callable(this, nameof(OnUnitAttacked)));
        SignalBus.Instance.Connect("UnitDefend", new Callable(this, nameof(OnUnitDefend)));

        //attack related
        //_attackRangeDetector = new Area2D();
        //var shape = new CollisionShape2D();
        GD.Print($"Creating attack range detector for {this.Name} with AttackRange: {AttackRange}");
        //shape.Shape = new CircleShape2D { Radius = AttackRange * 2 };
        // _attackRangeDetector.AddChild(shape);
        //AddChild(_attackRangeDetector);
        //change the _collisionShape radius to AttackRange

        // this.AreaEntered += OnAttackRangeEntered;
        //_attackRangeDetector = new Area2D();
        //  var shape = _collisionShape.Shape as CircleShape2D;
        //  shape.Radius = AttackRange;
        //GD.Print($"attack range detector for {this.Name} with AttackRange: {shape.Radius}");
        this.AreaEntered += OnAttackRangeEntered;
        this.AreaExited += BaseUnit_AreaExited;


        //_movement = GetNode<UnitMovement>("Movement");
        //_visuals = GetNode<UnitVisuals>("Visuals");

        // Find all ability nodes
        foreach (Node child in GetChildren())
        {
            //if (child is UnitAbility ability)
            //{
            //    _abilities.Add(ability);
            //    ability.Initialize(this);
            //}
        }
    }

    private void BaseUnit_AreaExited(Area2D area)
    {
        GD.Print($"Area {area.Name} exited the attack range of {this.Name}");
        var tween = CreateTween();
        tween.TweenProperty(area, "modulate:a", 1.0f, 1.0f).SetEase(Tween.EaseType.InOut);
    }

    private void OnAttackRangeEntered(Area2D area)
    {
        if (area == this) return;



        if (area is BaseUnit unit && unit != this)
        {
            if (unit.UnitTeam == this.UnitTeam)
            {
                GD.Print($"Ally {unit.Name} entered the attack range of {this.Name}");
                return; // Ignore allies
            }

            GD.Print($"Unit {unit.Name} entered the attack range of {this.Name}");


            var tween = CreateTween();
            //make area fade in and out
            tween.TweenProperty(area, "modulate:a", 0.5f, 0.5f).SetEase(Tween.EaseType.In);
            tween.TweenProperty(area, "modulate:a", 1.0f, 1.0f).SetEase(Tween.EaseType.Out);

            // Handle interaction with other units here, e.g., attack or show info
            // SignalBus.Instance.EmitSignal("UnitAttacked", this, unit);
        }
        else
        {
            GD.Print($"Area {area.Name} entered the attack range of {this.Name}");
        }
    }

    public override void _Input(InputEvent @event)
    {
        //temp reset of movements left for testing purposes
        if (Input.IsActionJustPressed("end"))
        {
            MovementsLeft = 6; // Reset movements left
                               //drawMovement(); //redraw/update movement path
        }

        if (Input.IsActionJustPressed("initialize"))
        {
            var sprite = GetNode<Sprite2D>("/root/Main/GameWorld/Units/BaseUnit/Sprite2D");



            ScalePlayerToCellSize();
            PlacePlayerRandomly();
        }

        if (Input.IsActionJustPressed("all"))
        {
            var units =  _gridManager.GetOccupiedCells();
            //print all units and their positions
            foreach (var unit in units)
            {
                GD.Print($"Unit {unit.Key.Name} is at grid position {unit.Value}");
            }
        }
    }

    public virtual void OnNewTurn()
    {
        HasMoved = false;
        HasActed = false;
    }

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        //  _visuals.PlayDamageEffect();
        _healthBar.Value = CurrentHealth;
        SignalBus.Instance.EmitSignal("UnitHealthChanged", this);
        GD.Print($"Unit {this.Name} took {amount} damage, current health: {CurrentHealth}");
        if (CurrentHealth <= 0)
        {
            GD.Print($"Unit {this.Name} has died.");
            Die();
        }
    }

    public void OnUnitAttacked(BaseUnit attacker, BaseUnit target)
    {
        if (attacker == null || target == null || attacker == target) return;

        if (target == this && MovementsLeft > 0)
        {
            GD.Print($"Unit {attacker.Name} attacked {target.Name}");
            // Handle attack logic here, e.g., apply damage
            this.TakeDamage(attacker.Attack);
            SignalBus.Instance.EmitSignal("UnitDefend", target, attacker);
        }
    }

    public void OnUnitDefend(BaseUnit attacker, BaseUnit target)
    {
        if (attacker == null || target == null || attacker == target) return;

        if (target == this)
        {
            GD.Print($"Unit {attacker.Name} defended against {target.Name}");
            // Handle attack logic here, e.g., apply damage
            this.TakeDamage(attacker.Attack);
        }
    }

    protected virtual void Die()
    {
        SignalBus.Instance.EmitSignal("UnitDied", this);
        var tween = CreateTween();
        // Fade out the unit before removing it using tween
        tween.TweenProperty(this, "modulate:a", 0.1f, 5.1f);
        QueueFree();
    }

    //public bool CanUseAbility(UnitAbility ability)
    //{
    //    return !HasActed && _abilities.Contains(ability) && ability.CanBeUsed();
    //}

    //public void UseAbility(UnitAbility ability, BaseUnit target)
    //{
    //    if (CanUseAbility(ability))
    //    {
    //        ability.Execute(target);
    //        HasActed = true;
    //    }
    //}

    public override void _Process(double delta)
    {
        if (_gridManager == null)
        {
            _gridManager = GetNode<GridManager>("/root/Main/GameWorld/Map");
        }

        if (_grid == null)
            _grid = _gridManager.GetGrid();
        if (_hasBeenPlaced) return;
        GD.Print($"Processing placement for player {this.Name}.");
        PlacePlayerRandomly();
        // Handle player input or other logic here
    }

    private void ScalePlayerToCellSize()
    {
        GD.Print($"Scaling player {this.Name} to CellSize: {_gridManager.GetCellSize()}, current size is {this.Portrait.Scale}");
        // Scale sprite (if texture isn't already CellSize x CellSize)
        if (this.Portrait == null || this.Portrait.Texture == null)
        {
            GD.PrintErr("Portrait or its texture is null, trying to get sprite2d");
            var portrait = GetNode<Sprite2D>($"/root/Main/GameWorld/Units/BaseUnit/Sprite2D");
            this.Portrait.Texture = portrait.Texture;
        }

        float scaleFactor = (float)_gridManager.GetCellSize() / this.Portrait.Texture.GetWidth();
        this.Portrait.Scale = new Vector2(scaleFactor, scaleFactor);
        GD.Print($"player {this.Name} scaled to size {this.Portrait.Scale}");

        // Resize CircleShape2D radius to half of CellSize (for tight fit)
        var circleShape = (CircleShape2D)CollisionShape.Shape;
        circleShape.Radius = _gridManager.GetCellSize() / 2.0f;
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
        else if (_hasBeenPlaced)
        {
            GD.PrintErr($"Player {this.Name} has been placed");
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

    private void OnUnitMoved(BaseUnit unit, int movementsLeft)
    {
        if (unit != this)
            return;

        if (_isSelected)
        {
            this.MovementsLeft = movementsLeft;
            GD.Print($"Unit {unit.Name} moved  while emitting a signal");
            // Update UI or other systems 
        }
    }

    private void OnUnitDeselected(BaseUnit baseUnit)
    {
        if (baseUnit != this)
            return;
        if (_isSelected)
        {
            _isSelected = false;
            GD.Print($"Unit deselected, hiding movement range. in {this}");
        }
        else
        {
            GD.Print($"Unit {this.UnitName} is not selected, no action taken.");
        }
    }

    //on input event
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                if (!_isSelected)
                {
                    _isSelected = true;
                    GD.Print($"Player {this.Name} clicked from input event ");
                    SignalBus.Instance.EmitSignal("UnitSelected", this);
                }
            }

            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed && _isSelected)
            {
                _isSelected = false;
                GD.Print("Unit deselected from input event.");
                SignalBus.Instance.EmitSignal("UnitDeselected");
            }
        }
    }

    public Sprite2D GetSprite()
    {
        return Portrait;
    }

    private void UpdateTeamSprite()
    {
        GD.Print($"Updating team sprite for {UnitName} with team {UnitTeam} for {Portrait}");
        switch (UnitTeam)
        {
            case Team.Neutral:
                Portrait.Texture = NeutralSprite;
                break;
            case Team.Player:
                Portrait.Texture = PlayerSprite;
                break;
            case Team.Enemy:
                Portrait.Texture = EnemySprite;
                break;
            case Team.Ally:
                Portrait.Texture = AllySprite;
                break;
            default:
                GD.PrintErr("Unknown team type, using neutral sprite.");
                Portrait.Texture = NeutralSprite;
                break;
        }
    }

    //on area entered
    public void _OnAreaEntered(Area2D area)
    {
        GD.Print($"Area {area.Name} entered the area of {this.Name}");
        if (area is BaseUnit unit && unit != this)
        {
            GD.Print($"Unit {unit.Name} entered the area of {this.Name}");
            // Handle interaction with other units here, e.g., attack or show info
            // SignalBus.Instance.EmitSignal("UnitSelected", unit);
        }
    }
}
