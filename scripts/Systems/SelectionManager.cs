using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;

public partial class SelectionManager : Node2D
{
    private BaseUnit _activeUnit;
    private GridPathfinding _gridPathfinding => GetNode<GridPathfinding>("/root/Main/GameWorld/GridPathfinding");
    private GridManager _gridManager => GetNode<GridManager>("/root/Main/GameWorld/Map");

    public bool IsEnabled { get; private set; } = true;

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }

    public override void _Ready()
    {
        GD.Print("SelectionManager ready");
        SignalBus.Instance.UnitDeselected += DeselectCurrent;
        SignalBus.Instance.UnitSelected += SelectUnit;
        SignalBus.Instance.UnitsGrouped += OnGrouped;
    }

    private void OnGrouped(ArmyStack armyStack)
    {
        //TODO anything else?
        DeselectCurrent();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsEnabled) return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            var clickedPosition = _gridPathfinding.GetLocalMousePosition();

            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                HandleLeftClick(clickedPosition);
            }
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                DeselectCurrent();
            }
        }
    }

    private void DeselectCurrent()
    {
        if (_activeUnit == null )
            return;

        _activeUnit._isSelected = false;
        _activeUnit = null;
        SignalBus.Instance.EmitSignal("UnitDeselected");
        
    }

    private void HandleLeftClick(Vector2 clickedPosition)
    {
        var clickedObject = GetObjectsAtMouse(clickedPosition);
        //TODO handle cases where other objects are clicked (e.g. obstacles, items, etc.)
        //what about attack?
        if (clickedObject is BaseUnit clickedUnit)
        {
            GD.Print($" Clicked on unit: {clickedUnit.Name} of type {clickedUnit.GetType()}");
            if (_activeUnit == null)
            {
                if (clickedUnit.UnitTeam == BaseGameObject.Team.Enemy)
                {
                    GD.Print("clicked on enemy unit, can't be selected");
                    return;
                }

                SelectUnit(clickedUnit);
            }
            //TODO if unit is already selected, attack, group, interact, etc. depending on the context
            //if (clickedUnit.UnitTeam == _activeUnit.UnitTeam)
            //    SignalBus.Instance.EmitSignal("FriendlyUnitClicked", clickedUnit);
            //if (clickedUnit.UnitTeam != _activeUnit.UnitTeam)
            //    SignalBus.Instance.EmitSignal("EnemyUnitClicked", clickedUnit);
            if (_activeUnit != clickedUnit)
                SignalBus.Instance.EmitSignal("MapPointClicked", clickedPosition); 
        }
        else
        {
            SignalBus.Instance.EmitSignal("MapPointClicked", clickedPosition);
        }

    }

    private void SelectUnit(BaseUnit clickedUnit)
    {
        if (_activeUnit == clickedUnit)
            return;

        clickedUnit._isSelected = true;
        _activeUnit = clickedUnit;
        CameraManager.Instance.FocusOnUnit(_activeUnit);
        GD.Print($"in selection manager - Unit {clickedUnit.Name} selected at position {clickedUnit.Position}");
        SignalBus.Instance.EmitSignal("UnitSelected", clickedUnit);
    }

    private BaseGameObject GetObjectsAtMouse(Vector2 clickedPosition)
    {
        return _gridManager.IsCellOccupied(clickedPosition);
    }
}
