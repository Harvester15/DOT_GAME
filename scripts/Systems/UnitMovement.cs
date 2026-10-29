using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;
using System.Linq;

public partial class UnitMovement : Node
{
    [Export] public BaseUnit Owner;
    public int MovementsLeft;  // Default movement range
    private GridPathfinding _pathfinding;
    private MovementRangeVisualizer _movementRangeVisualizer => GetNode<MovementRangeVisualizer>("/root/Main/UI/MovementRangeVisualizer");
    private BaseUnit _selectedUnit;
    private bool _isSelected = false;
    private Vector2[] _pathSelected;
    private bool _isMoving = false;
    private InputEvent _inputEvent;
    private Vector2 _clickedPosition;
    private bool _isAttacking = false;

    private GridManager _gridManager => GetNode<GridManager>("/root/Main/GameWorld/Map");
    private Camera2D _camera => GetTree().Root.GetNode<Camera2D>("/root/Main/CameraRig/Camera2D");
    private GridPathfinding _gridPathfinding => GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        GD.Print("UnitMovement: Connected to UnitSelected signal.");
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
        GD.Print($"ready called by: {this.GetPath()}");

        SignalBus.Instance.UnitSelected += OnUnitSelected;
        SignalBus.Instance.UnitDied += OnUnitDied;
        SignalBus.Instance.MapPointClicked += OnMapPointClicked;
        SignalBus.Instance.UnitDeselected += OnUnitDeselected;
    }

    private void OnUnitDeselected()
    {
        _pathSelected = null;
    }

    private void OnMapPointClicked(Vector2 position)
    {
        if (_selectedUnit == null)
            return;

        if (_pathSelected != null)
        {
            if (IsClickedPointOnPath(position))
                StartMovement(_pathSelected);
            else
                GetNewPath(position);
        }
        if (_pathSelected == null)
        {
            GetNewPath(position);
        }
    }

    private void GetNewPath(Vector2 position)
    {
        _pathSelected = _pathfinding.GetMovementPath(_selectedUnit, position);
        GD.Print($"Path for unit {_selectedUnit.Name}: {_pathSelected}");
        _movementRangeVisualizer.ShowMovement(_pathSelected, _selectedUnit);
    }

    private async void StartMovement(Vector2[] path)
    {
        SetProcessUnhandledInput(false); // Disable input during movement

        foreach (Vector2 nextPoint in path)
        {
            if (_selectedUnit.MovementsLeft <= 0)
            {
                StoppedMoving($"IN startmovement - no more movementpoints Reached position: {nextPoint}");
                break;
            }

            var occupant = _gridManager.IsCellOccupied(nextPoint);
            if (occupant != null)
            {
                GD.Print($"IN startmovement - Position {nextPoint} is occupied by {occupant.Name}, cannot move there.");
                HandleCombat(occupant);
                break;
            }
            // Move towards the next point
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(_selectedUnit, "position", nextPoint, 0.15f).SetTrans(Tween.TransitionType.Linear);

            await ToSignal(tween, "finished");
            _selectedUnit.MovementsLeft--;

            SignalBus.Instance.EmitSignal("UnitMoved", _selectedUnit, _selectedUnit.MovementsLeft);
        }
        StoppedMoving("IN startmovement - Finished moving along path.");
        SetProcessUnhandledInput(true); // Re-enable input after movement
    }

    private void HandleCombat(BaseUnit target)
    {
        if (_selectedUnit.UnitTeam != target.UnitTeam)
        {
            GD.Print("Attacking enemy!");
            SignalBus.Instance.EmitSignal("UnitAttacked", _selectedUnit, target);
            _selectedUnit.MovementsLeft--;
            SetProcessInput(true);
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void StoppedMoving(string msg = null)
    {
        if (!string.IsNullOrEmpty(msg))
            GD.Print($"{msg}");
        _isMoving = false; // Stop moving if we reached the clicked position
        _movementRangeVisualizer.ShowMovement(new Vector2[0], _selectedUnit);
        _gridManager.UnregisterUnitFromCell(_selectedUnit);
        _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(_selectedUnit.GlobalPosition, _gridManager.GetCellSize()), _selectedUnit);
    }

    private bool IsClickedPointOnPath(Vector2 pointClicked)
    {
        var currentPointOnGrid = Utils.WorldToGrid(pointClicked, _gridManager.GetCellSize());
        var currentPathOnGrid = Utils.WorldToGrid(_pathSelected, _gridManager.GetCellSize());
        GD.Print($"Current point ON GRID clicked: {currentPointOnGrid}, this is inside path ON GRID {currentPathOnGrid.Contains(currentPointOnGrid)}");
        return currentPathOnGrid.Contains(currentPointOnGrid);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
        }
    }

    private void MoveAlongPath(Vector2[] pathSelected)
    {
        if (_pathSelected.Length > 0 && !_isMoving) //use _movementPath from visualizer 
            return;
    }

    public void OnUnitSelected(BaseUnit selectUnit)
    {
        GD.Print($"unit throwing signal {selectUnit}");

        _selectedUnit = selectUnit;
    }

    public void OnUnitDied(BaseUnit unit)
    {
        GD.Print($"Unit died: {unit.Name}");
        if (_selectedUnit == unit)
        {
            _isSelected = false;
            _selectedUnit = null;
            _pathSelected = null;
            _isMoving = false;
            _movementRangeVisualizer.ShowMovement(new Vector2[0], null);
            GD.Print("Unit deselected due to death.");
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (Owner.HasMoved)
        {
            GD.Print("Unit has already moved this turn.");
            return;
        }
        //TODO : validate move
        if (true)//(IsValidMove(targetPosition))
        {
            Owner.GridPosition = targetPosition;
            Owner.HasMoved = true;
            GD.Print($"Unit {Owner.UnitName} moved to {targetPosition}");
            // Update visuals, animations, etc.
            //  Owner._visuals.UpdatePosition();
        }
        else
        {
            GD.Print("Invalid move.");
        }
    }
}
