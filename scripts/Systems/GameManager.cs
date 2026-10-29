using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Systems;
using Godot;
using System;

namespace dot_battle_signal_bus.scripts.Systems;

public enum GameState { Map, Combat, City, Menu }

public partial class GameManager : Node
{

public static GameManager Instance { get; private set; }
public GameState CurrentState { get; private set; } = GameState.Map;

// References to your other managers (assigned in Inspector or _Ready)
[Export] public CombatManager Combat;
[Export] public CameraManager Camera;
[Export] public SelectionManager Selection;

public override void _Ready()
{
    Instance = this;
}

public void EnterCombat(CombatContext context)
{
    CurrentState = GameState.Combat;

    // 1. Disable Map interactions
    Selection.SetEnabled(false);

    // 2. Trigger UI and System
   // Combat.StartBattle(context);

    GD.Print("Game State: COMBAT");
}

public void ExitCombat()
{
    CurrentState = GameState.Map;
    Selection.SetEnabled(true);
    GD.Print("Game State: MAP");
}
}
