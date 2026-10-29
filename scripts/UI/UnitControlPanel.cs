using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitControlPanel : Panel
{
    private GridManager _gridManager => GetNode<GridManager>("/root/Main/GameWorld/Map");
    private List<BaseUnit> _playerUnits = new List<BaseUnit>();
    private int _currentUnitIndex = -1;
    private bool _isConnectedToUnitDied = false;
    private bool _isConnectedToUnitEnteredAlly = false;
    private bool _isConnectedToUnitExitedAlly = false;
    private BaseUnit _enteredUnit;
    private BaseUnit _enteringUnit;

    private TextureButton _groupUnitsBtn => GetNode<TextureButton>("HBoxContainer/GridContainer/GroupUnitsBtn");
    private TextureButton _ungroupUnitsBtn => GetNode<TextureButton>("HBoxContainer/GridContainer/UnGroupUnitsBtn");


    public override void _Ready()
    {
        GD.Print("getting player units for unit control panel");
        GetPlayerUnits();
        _ungroupUnitsBtn.Visible = false;
        SignalBus.Instance.UnitAttacked += OnUnitAttacked;
        SignalBus.Instance.CombatEnded += OnCombatEnded;
        // connect to signal unitdied, if the current unit dies, move to the next one

    }

    private void OnCombatEnded(CombatResult combatResult)
    {
        Visible = true;
    }

    private void OnUnitAttacked(BaseUnit attacker, BaseUnit target)
    {
        Visible = false;
    }

    public override void _Process(double delta)
    {
        // For testing: Press N to cycle through player units
        if (!_isConnectedToUnitDied)
        {
            SignalBus.Instance.Connect("UnitDied", new Callable(this, nameof(OnUnitDied)));
            _isConnectedToUnitDied = SignalBus.Instance.IsConnected(SignalBus.SignalName.UnitDied, new Callable(this, nameof(OnUnitDied)));
            GD.Print($"Connected to UnitDied signal: {_isConnectedToUnitDied}");
        }
        if (!_isConnectedToUnitEnteredAlly)
        {
            SignalBus.Instance.Connect("UnitEnteredAlly", new Callable(this, nameof(UnitEnteresAlly)));
            _isConnectedToUnitEnteredAlly = SignalBus.Instance.IsConnected(SignalBus.SignalName.UnitEnteredAlly, new Callable(this, nameof(UnitEnteresAlly)));
            GD.Print($"Connected to UnitEnteresAlly signal: {_isConnectedToUnitEnteredAlly}");
        }

        if (!_isConnectedToUnitExitedAlly)
        {
            SignalBus.Instance.Connect("UnitExitedAlly", new Callable(this, nameof(UnitExitsAlly)));
            _isConnectedToUnitExitedAlly = SignalBus.Instance.IsConnected(SignalBus.SignalName.UnitExitedAlly, new Callable(this, nameof(UnitExitsAlly)));
            GD.Print($"Connected to UnitExitsAlly signal: {_isConnectedToUnitExitedAlly}");
        }

    }


    private void GetPlayerUnits()
    {
        _playerUnits.Clear();

        // 1. Get occupied cells and gather BaseUnit instances
        var occupied = _gridManager.GetOccupiedCells();
        var units = occupied.Values.OfType<BaseGameObject>();

        // 2. Find units that belong to the player's team using several heuristics via reflection
        foreach (var unit in occupied)
        {
            if (((BaseUnit)unit.Key).UnitTeam == BaseUnit.Team.Player)
            {
                _playerUnits.Add(unit.Key as BaseUnit);
            }
        }
        GD.Print($"Found {_playerUnits.Count} player unit(s) on the map.");
    }

    private void _on_next_unit_btn_pressed()
    {
        GetPlayerUnits();
        if (_playerUnits.Count < 0)
        {
            GD.Print("No current unit selected, cannot move to next unit.");
            return;
        }

        GD.Print("Next Unit button pressed");
        if (_playerUnits.Count > 0)
        {
            GD.Print($"Current unit index before increment: {_currentUnitIndex}");
            _currentUnitIndex = (_currentUnitIndex + 1) % _playerUnits.Count;
            CameraManager.Instance.FocusOnUnit(_playerUnits[_currentUnitIndex]);
            SignalBus.Instance.EmitSignal(nameof(SignalBus.UnitSelected), _playerUnits[_currentUnitIndex]);
            SignalBus.Instance.EmitSignal(nameof(SignalBus.NextUnitClicked), _playerUnits[_currentUnitIndex]);
        }

        // Emit a signal to move to the next unit in the turn order
        //SignalBus.Instance.EmitSignal(nameof(SignalBus.NextUnitInTurnOrder));
    }

    private void _on_group_units_btn_pressed()
    {
        GD.Print("grouping units");
        if (_enteredUnit != null && _enteringUnit != null)
        {
            //TODO might be able to recruit neutrals? or will just have neutrals switch side?
            if (_enteredUnit.UnitTeam != _enteringUnit.UnitTeam)
            {
                GD.Print("Cannot group units from different teams!");
                return;
            }
            ArmyStack armyStack;

            GD.Print($"Grouping {_enteredUnit.UnitName} and {_enteringUnit.UnitName}");
            // TODO Implement your grouping logic here, e.g., create a new GroupedUnit that contains both units
            // You might want to emit a signal or call a method on the units to handle the grouping
            if (_enteredUnit is ArmyStack)
            {
                armyStack = (ArmyStack)_enteredUnit;
                _gridManager.UnregisterUnitFromCell(_enteringUnit);
                armyStack.AddUnit(_enteringUnit);

            }
            else if (_enteringUnit is ArmyStack)
            {
                armyStack = (ArmyStack)_enteringUnit;
                _gridManager.UnregisterUnitFromCell(_enteredUnit);
                armyStack.AddUnit(_enteredUnit);
            }
            else
            {
                _gridManager.UnregisterUnitFromCell(_enteringUnit);
                _gridManager.UnregisterUnitFromCell(_enteredUnit);
                armyStack = new ArmyStack();
                armyStack.CreateUnit(_enteringUnit, _enteredUnit);
                armyStack.InputPickable = true;
                armyStack.Position = armyStack.Units[0].Position;
                armyStack.UnitTeam = armyStack.Units[0].UnitTeam;

                _gridManager.RegisterUnitOnCell(Utils.WorldToGrid(armyStack.Position, _gridManager.GetCellSize()), armyStack);
                GD.Print($"Created new ArmyStack in {armyStack.Position} and  a global position:{armyStack.GlobalPosition}");
                if (armyStack.Units.Count == 0)
                {
                    GD.Print("Error: ArmyStack has no units after creation!");
                    return;
                }
                //GD.Print($"Moved UnitControlPanel to {armyStack.Position} and  a global position:{armyStack.GlobalPosition}");
            }
            GetPlayerUnits();
            _ungroupUnitsBtn.Visible = true;

            SignalBus.Instance.EmitSignal("UnitsGrouped", armyStack);
            SignalBus.Instance.EmitSignal("UnitDeselected");
        }
    }

    public void _on_ungroup_units_btn_pressed()
    {
        GD.Print("ungrouping units");
        _ungroupUnitsBtn.Visible = false;

    }

    public void _on_end_turn_btn_pressed()
    {
        GD.Print("End Turn button pressed");
        // Emit a signal to end the player's turn
        SignalBus.Instance.EmitSignal("UnitDeselected");
        TurnManager.Instance.EndTurn();
    }

    public void UnitEnteresAlly(BaseUnit unitEntered, BaseUnit enteringUnit)
    {
        //TODO might be able to recruit neutrals? or will just have neutrals switch side?
        //TODO if entered ally, have an attack button, perhaps a challange(if player and enemy units support challange)?
        if (!unitEntered.IsGrouped && !enteringUnit.IsGrouped && unitEntered.UnitTeam == enteringUnit.UnitTeam)
        {
            GD.Print("in unitcontrolpanel - unit entered ally");
            _groupUnitsBtn.Visible = true;
            _enteredUnit = unitEntered;
            _enteringUnit = enteringUnit;
        }
    }

    public void UnitExitsAlly(BaseUnit unitEntered, BaseUnit enteringUnit)
    {
        GD.Print("in unitcontrolpanel - unit entered ally");
        _groupUnitsBtn.Visible = false;
        _enteredUnit = null;
        _enteringUnit = null;
    }

    public void OnUnitDied(BaseUnit baseUnit)
    {
        GD.Print("in unitcontrolpanel - unit died");
        _playerUnits.Remove(baseUnit);
    }
}
