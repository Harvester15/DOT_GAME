using dot_battle_signal_bus.scripts;
using Godot;
using System;
using System.Data;

public partial class UnitInfo : PanelContainer
{

    public override void _Ready()
    {
        // Connect to global signals
        SignalBus.Instance.Connect(nameof(SignalBus.UnitSelected), new Callable( this, nameof(OnUnitSelected)));
        SignalBus.Instance.Connect(nameof(SignalBus.UnitDeselected), new Callable(this, nameof(OnUnitDeselected)));
        SignalBus.Instance.Connect(nameof(SignalBus.UnitHealthChanged), new Callable(this, nameof(UpdateAllInfo)));
    }

    private void OnUnitSelected(BaseUnit unit)
    {
        this.Visible = true; // Show the unit info panel

        // Subscribe to unit-specific signals
        //unit.Connect(nameof(SignalBus.uni), this, nameof(UpdateHealthBar)));
        //unit.Connect(nameof(BaseUnit.UnitStatusChanged), this, nameof(UpdateStatuses)));

        // Initial update
        UpdateAllInfo(unit);
    }

    private void OnUnitDeselected(BaseUnit unit)
    {
        // Unsubscribe from unit-specific signals
        //unit.Disconnect(nameof(BaseUnit.UnitHealthChanged), this, nameof(UpdateHealthBar));
        //unit.Disconnect(nameof(BaseUnit.UnitStatusChanged), this, nameof(UpdateStatuses));

        // Clear UI elements
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/UnitName").Text = "";
        GetNode<TextureRect>("MarginContainer/VBoxContainer/Portrait").Texture = null;
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/HealthPoints").Text = "";
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/MovementPoints").Text = "";
        // ... etc
        this.Visible = false; // Hide the unit info panel
    }

    private void UpdateAllInfo(BaseUnit unit)
    {
        // Update all UI elements
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/UnitName").Text = unit.UnitName;
        GetNode<TextureRect>("MarginContainer/VBoxContainer/Portrait").Texture = unit.Portrait.Texture;
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/HealthPoints").Text = unit.CurrentHealth.ToString();
        GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/MovementPoints").Text = unit.MovementsLeft.ToString();
        // ... etc
    }
}
