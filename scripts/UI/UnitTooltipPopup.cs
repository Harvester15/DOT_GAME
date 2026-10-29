using Godot;
using System;

public partial class UnitTooltipPopup : PanelContainer
{
    public void Setup(BaseUnit unit)
    {
        var namelabel = GetNode<Label>("VBoxContainer/UnitNameLabel");

        GetNode<Label>("VBoxContainer/UnitNameLabel").Text = unit.UnitName;
        GetNode<Label>("VBoxContainer/HealthLabel").Text = $"HP: {unit.CurrentHealth}/{unit.MaxHealth}";
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
