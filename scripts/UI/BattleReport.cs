using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Combat.Interface;
using dot_battle_signal_bus.scripts.Combat.Systems;
using Godot;
using System;
using System.ComponentModel.Design;

public partial class BattleReport : CanvasLayer
{
	private RichTextLabel _report;// => GetNode<RichTextLabel>("BattleReport/RichTextLabel");
	private Label _xp;// => GetNode<Label>("BattleReport/XPLabel");
	private GridContainer _loserGrid;// => GetNode<GridContainer>("BattleReport/GridContainer");
	private bool _isInitialized = false;

	public override void _Ready()
	{
		GD.Print("BATTLEERPORT - BattleReport ready");
		Visible = false; // Hide the battle report by default
						 // Connect to global signals

		Initialize();
	}

	private void Initialize()
	{
		if (_report == null)
			_report = GetNode<RichTextLabel>("BattleReport/RichTextLabel");
		if (_xp == null)
			_xp = GetNode<Label>("BattleReport/XPLabel");
		if (_loserGrid == null)
			_loserGrid = GetNode<GridContainer>("BattleReport/GridContainer");
		_isInitialized = true;
	}

	public void Display(CombatResult combatResult)
	{
		GD.Print("BATTLERPORT - Combat ended, showing battle report");
		if (!_isInitialized)
			Initialize();

		if (combatResult.Winners == null || combatResult.Losers == null)
		{
			GD.Print($"winners - {combatResult.Winners} or loosers - {combatResult.Losers} are null");
			return;
		}

		foreach (var entry in combatResult.Losers)
		{
			CombatSlot unitSlot = new();
			//var unitSlotScene = GD.Load<PackedScene>("res://scenes/UI/unit_report_slot.tscn");
			//var unitSlot = unitSlotScene.Instantiate<TextureRect>();

			unitSlot.Portrait = entry.Portrait.Duplicate() as TextureRect;
			var bar = unitSlot.Portrait.GetChildren();
			unitSlot.Portrait.RemoveChild(bar[0]);
			_loserGrid.AddChild(unitSlot);
		}
		_xp.Text = $"XP Gained: {combatResult.TotalXPPool}";

		GD.Print($"making the BATTLERPORT visible");
		Visible = true;
	}

	public void _on_x_btn_pressed()
	{
		GetTree().Paused = false;
		_isInitialized = false;
		QueueFree();
		Visible = false;
		SignalBus.Instance.EmitSignal("BattleReportClosed");
	}


}

