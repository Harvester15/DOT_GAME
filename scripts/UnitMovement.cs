using dot_battle_signal_bus.scripts;
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


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SignalBus.Instance.Connect("UnitSelected", new Callable(this, nameof(OnUnitSelected)));
        GD.Print("UnitMovement: Connected to UnitSelected signal.");
        _pathfinding = GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
        GD.Print($"ready called by: {this.GetPath()}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_isMoving && _selectedUnit?.MovementsLeft > 0 && _pathSelected != null && _pathSelected.Length > 0)
        {
            // Move player towards the next ghost unit if available
            GD.Print($"Moving player towards ghost units, movements left: {_selectedUnit.MovementsLeft}");
            GD.Print($"Moving player towards ghost units, movementpath left: {_pathSelected?.Length}");

            var Movements = _pathSelected.Chunk(_selectedUnit.MovementRange).ToArray();

            if (Movements.Length > 0)
            {
                if (_pathSelected.Length > 0 && IsClickedPointOnPath() && _selectedUnit.MovementsLeft > 0)
                {
                    var unit = _gridManager.IsCellOccupied((_pathSelected[0]));
                    //check if _pathSelected[0] is occupied by another unit
                    if (unit != null)
                    {
                        GD.Print($"Position {_pathSelected[0]} is occupied, cannot move there.");
                        StoppedMoving($"Cannot move to occupied position: {_pathSelected[0]}");
                        GD.Print("Is attacking");
                        _selectedUnit.MovementsLeft--;
                        SignalBus.Instance.EmitSignal("UnitAttacked", _selectedUnit, _gridManager.IsCellOccupied((_pathSelected[0])));
                        return;
                    }

                    _selectedUnit.Position = _selectedUnit.Position.MoveToward(_pathSelected[0], 6);
                    GD.Print($"still moving, unit position {_selectedUnit.Position}");
                    GD.Print($"Moved to next point, movements left: {_selectedUnit.MovementsLeft}");
                    if (_selectedUnit.Position == _pathSelected[0])
                    {
                        GD.Print($"Reached next point: {_pathSelected[0]}");
                        var path = _pathSelected.ToList();
                        path.RemoveAt(0);
                        _pathSelected = path.ToArray(); // Remove the first point after moving
                        SignalBus.Instance.EmitSignal("UnitMoved", _selectedUnit, _selectedUnit.MovementsLeft);
                        _selectedUnit.MovementsLeft--;
                        return;
                    }
                    if (_selectedUnit.Position == _clickedPosition)
                    {
                        StoppedMoving($"Reached clicked position: {_clickedPosition}");
                    }
                }
                else if (_pathSelected.Length == 0 && _isMoving)
                {
                    StoppedMoving("Movement finished, resetting path ghosts.");
                }
            }
        }

        if (_pathSelected?.Length == 0 && _isMoving)
        {
            StoppedMoving("Movement finished, resetting path ghosts.");
        }

        if (_selectedUnit?.MovementsLeft == 0 && _isMoving)
        {
            if (_selectedUnit != null)
            {
                StoppedMoving("No movements left, stopping movement.");
                _isMoving = false; // Stop moving if no movements left
            }
        }

        //attack
        if (_pathSelected != null)
            if (!_isMoving && _selectedUnit != null && _isAttacking)
                if (IsClickedPointOnPath()
                && _gridManager.IsCellOccupied(_clickedPosition) != null)
                {
                    GD.Print($"attacking");
                    SignalBus.Instance.EmitSignal("UnitAttacked", _selectedUnit, _gridManager.IsCellOccupied((_clickedPosition)));

                    _isAttacking = false; // Reset attacking state after attack
                    _selectedUnit.MovementsLeft--;

                }


    }

    private void StoppedMoving(string msg = null)
    {
        if (!string.IsNullOrEmpty(msg))
            GD.Print($"{msg}");
        _isMoving = false; // Stop moving if we reached the clicked position
        _movementRangeVisualizer.ShowMovement(new Vector2[0], _selectedUnit);
        _gridManager.UnregisterUnitFromCell(_selectedUnit);
        _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(_selectedUnit.Position, _gridManager.GetCellSize()), _selectedUnit);
    }

    private bool IsClickedPointOnPath()
    {
        var currentPointOnGrid = Utils.WorldToGrid(_clickedPosition, _gridManager.GetCellSize());
        var currentPathOnGrid = Utils.WorldToGrid(_pathSelected, _gridManager.GetCellSize());

        GD.Print($"Current point ON GRID clicked: {currentPointOnGrid}, this is inside path ON GRID {currentPathOnGrid.Contains(currentPointOnGrid)}");
        return currentPathOnGrid.Contains(currentPointOnGrid);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (_isSelected)
                {
                    _clickedPosition = GetViewport().GetMousePosition();
                }

                if ((_isSelected && _pathSelected == null) || (_isSelected && _pathSelected != null && !IsClickedPointOnPath()))
                {
                    _pathSelected = _pathfinding.GetMovementPath(_selectedUnit);
                    GD.Print($"Path for unit {_selectedUnit.Name}: {_pathSelected}");
                    _movementRangeVisualizer.ShowMovement(_pathSelected, _selectedUnit);
                }

                else if (_isSelected && _pathSelected != null && _pathSelected.Length == 1
                    && _gridManager.IsCellOccupied(_clickedPosition) != null)
                {
                    GD.Print($"will attack - {_isAttacking}");
                    _isAttacking = true;
                }

                else if (_isSelected && _pathSelected != null && _pathSelected.Length > 0 && !_isMoving)
                {
                    _isMoving = true;
                    GD.Print($"Moving along path for unit {_selectedUnit.Name}: {_pathSelected}");
                }



            }
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                if (_isSelected)
                {
                    _isSelected = false;
                    _pathSelected = null;
                    _isMoving = false;
                    GD.Print("Unit deselected, hiding movement range.");
                    _movementRangeVisualizer.ShowMovement(new Vector2[0], _selectedUnit);
                    SignalBus.Instance.EmitSignal("UnitDeselected", _selectedUnit);
                    _selectedUnit = null;
                }
            }
        }
    }

    private void MoveAlongPath(Vector2[] pathSelected)
    {
        if (_pathSelected.Length > 0 && !_isMoving) //use _movementPath from visualizer 
            return;
    }

    //public void OnUnitSelected(BaseUnit selectUnit)
    public void OnUnitSelected(BaseUnit selectUnit)
    {
        GD.Print($"unit throwing signal {selectUnit}");


        if (!_isSelected)
        {
            if (_selectedUnit?.Owner == selectUnit.Owner) // 
                return;
            //why is this being hit multiple times???
            GD.Print($"Unit selected: {selectUnit.Name} signal cought");
            _selectedUnit = selectUnit;
            MovementsLeft = _selectedUnit.MovementsLeft;
            _isSelected = true;
            GD.Print($"unit now selected {_selectedUnit}, with {MovementsLeft} movementspoints left");

        }


    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (Owner.HasMoved)
        {
            GD.Print("Unit has already moved this turn.");
            return;
        }

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
