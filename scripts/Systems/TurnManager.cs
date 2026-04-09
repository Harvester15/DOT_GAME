// TurnManager.cs
using Godot;
using System.Collections.Generic;


namespace dot_battle_signal_bus.scripts;


public partial class TurnManager : Node
{
    // Turn phases
    public enum GameState { Initializing, PlayerTurn, EnemyTurn, Transitioning }

    [Signal] public delegate void StateChangedEventHandler(GameState newState);
    [Signal] public delegate void TurnChangedEventHandler(int turnNumber);

    private GameState _currentState = GameState.Initializing;
    private int _turnCount = 1;

    private static TurnManager _instance;
    public static TurnManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TurnManager();
            }
            return _instance;
        }
    }

    public override void _Ready()
    {

         // To register the singleton in Godot's autoload:
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            QueueFree(); // Ensure only one instance exists
        }

        GD.Print("TurnManager ready. Current state: " + _currentState);
        // Give the world time to load units
        CallDeferred(nameof(StartGame));
    }

    private void StartGame()
    {
        GD.Print("Game Starting...");
        ChangeState(GameState.PlayerTurn);
    }

    public void ChangeState(GameState newState)
    {
        _currentState = newState;
        EmitSignal(SignalName.StateChanged, (int)newState);

        switch (newState)
        {
            case GameState.PlayerTurn:
                GD.Print("Player's move!");
                break;
            case GameState.EnemyTurn:
                StartEnemyAI();
                break;
        }
    }

    private async void StartEnemyAI()
    {
        // HOMM style: wait a moment so the player sees the UI change
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        GD.Print("AI is thinking...");
        // Logic for AI movement here...

        // When AI is done:
        EndTurn();
    }

    public void EndTurn()
    {
        if (_currentState == GameState.PlayerTurn)
        {
            ChangeState(GameState.EnemyTurn);
        }
        else
        {
            _turnCount++;
            ChangeState(GameState.PlayerTurn);
            EmitSignal(SignalName.TurnChanged, _turnCount);
            
        }
    }
}