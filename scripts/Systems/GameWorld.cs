using dot_battle_signal_bus.scripts;
using Godot;
using System;

public partial class GameWorld : Node2D
{
	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		SignalBus.Instance.EmitSignal("WorldInitialized");
	}
}

