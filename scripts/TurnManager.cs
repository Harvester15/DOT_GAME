// TurnManager.cs
using Godot;
using System.Collections.Generic;


namespace dot_battle_signal_bus.scripts;


public partial class TurnManager : Node
{
    // Turn phases
    public enum TurnPhase
    {
        StartTurn,
        PlayerTurn,
        EnemyTurn,
        EndTurn
    }

    [Signal]
    public delegate void TurnPhaseChangedEventHandler(TurnPhase newPhase);

    private TurnPhase _currentPhase;
    private int _currentTurn = 0;
    private List<BaseUnit> _unitsInTurnOrder = new List<BaseUnit>();
    private int _currentUnitIndex = 0;

    public TurnPhase CurrentPhase
    {
        get => _currentPhase;
        private set
        {
            _currentPhase = value;
           //fix this
            // SignalBus.Instance.EmitSignal(nameof(TurnPhaseChanged), _currentPhase);
        }
    }

    public override void _Ready()
    {
        // Initialize turn system
        CurrentPhase = TurnPhase.StartTurn;
    }

    public void RegisterUnit(BaseUnit unit)
    {
        _unitsInTurnOrder.Add(unit);
        // You might want to sort units by initiative/speed here
    }

    public void UnregisterUnit(BaseUnit unit)
    {
        _unitsInTurnOrder.Remove(unit);
    }

    public void StartNextTurn()
    {
        _currentTurn++;
        CurrentPhase = TurnPhase.StartTurn;

        // Reset all units for new turn
        foreach (var unit in _unitsInTurnOrder)
        {
            unit.OnNewTurn();
        }

        _currentUnitIndex = 0;
        ProcessNextUnit();
    }

    private void ProcessNextUnit()
    {
        if (_currentUnitIndex >= _unitsInTurnOrder.Count)
        {
            // All units have acted, end turn
            CurrentPhase = TurnPhase.EndTurn;
            return;
        }

        var currentUnit = _unitsInTurnOrder[_currentUnitIndex];

        if (currentUnit.UnitTeam == BaseUnit.Team.Player)
        {
            CurrentPhase = TurnPhase.PlayerTurn;
            // Signal for player to control this unit
        }
        else
        {
            CurrentPhase = TurnPhase.EnemyTurn;
            // Start AI processing for this unit
        }
    }

    public void UnitFinishedTurn()
    {
        _currentUnitIndex++;
        ProcessNextUnit();
    }
}