using dot_battle_signal_bus.scripts;
using Godot;
using System;
using System.Data;

public partial class UnitInfo : PanelContainer
{
    private MarginContainer _UnitView => GetNode<MarginContainer>("SingleUnitContainer");
    private VBoxContainer _groupView => GetNode<VBoxContainer>("GroupContainer");
    private GridContainer _groupInfoGrid => GetNode<GridContainer>("GroupContainer/MarginContainer/ScrollContainer/GridContainer");

    private ArmyStack _armyStack;

    public override void _Ready()
    {
        this.Visible = false;
        _groupView.Visible = false;

        // Typed event subscriptions — compiler-checked signatures.
        SignalBus.Instance.UnitSelected += OnUnitSelected;
        SignalBus.Instance.UnitDeselected += OnUnitDeselected;
        SignalBus.Instance.UnitDied += OnUnitDied;          // correct BaseUnit-typed handler
        SignalBus.Instance.UnitHealthChanged += UpdateAllInfo;       // now matches 1-arg delegate
        SignalBus.Instance.UnitMoved += OnMoved;
        SignalBus.Instance.UnitsGrouped += OnGrouped;
    }

    public override void _ExitTree()
    {
        // Mirror cleanup so a freed panel can't be invoked.
        SignalBus.Instance.UnitSelected -= OnUnitSelected;
        SignalBus.Instance.UnitDeselected -= OnUnitDeselected;
        SignalBus.Instance.UnitDied -= OnUnitDied;
        SignalBus.Instance.UnitHealthChanged -= UpdateAllInfo;
        SignalBus.Instance.UnitMoved -= OnMoved;
        SignalBus.Instance.UnitsGrouped -= OnGrouped;
    }

    /// <summary>UnitDied carries the dead unit; hide the panel if it was shown.</summary>
    private void OnUnitDied(BaseUnit unit)
    {
        OnUnitDeselected();
    }

    private void OnMoved(BaseUnit unit, int movement)
    {
        UpdateAllInfo(unit);
    }

    private void OnGrouped(ArmyStack group)
    {
        ClearGroupInfo();
        

        GD.Print($"UNITINFO - Updating group info for: {group.UnitName} with a unit count of: {group.Units.Count}");
        foreach (var unit in group.Units)
        {
            TextureRect textureRect = unit.Portrait.Duplicate() as TextureRect;

            var tooltip = textureRect._MakeCustomTooltip("");
            //textureRect.TooltipText = unit.Name;
            HoverTooltipComponent toolTip = new HoverTooltipComponent();
            toolTip.Initialize(unit);


            textureRect.CustomMinimumSize = new Vector2(30, 30);
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
            var healthbar = textureRect.GetNode<ProgressBar>("HealthBar");
            healthbar.LayoutMode = 1;
            healthbar.AnchorsPreset = 12;            
            _groupInfoGrid.AddChild(textureRect);
        }
        _armyStack = group;
        UpdateAllInfo(group);
    }

    private void ClearGroupInfo()
    {
        GD.Print($"clearing groupinfo, current count: {_groupInfoGrid.GetChildCount()}");
        var group = _groupInfoGrid.GetChildren();
        foreach (var node in group)
        {
            _groupInfoGrid.RemoveChild(node);
        }
    }

    private void OnUnitSelected(BaseUnit unit)
    {
        this.Visible = true; // Show the unit info panel
        if (unit is ArmyStack)
        {
            _UnitView.Visible = false; // Hide single unit info
            _groupView.Visible = true; // Show group info panel
        }
        else
        {
            _UnitView.Visible = true; // Show single unit info
            _groupView.Visible = false; // Hide group info panel
        }

        // Subscribe to unit-specific signals
        //unit.Connect(nameof(SignalBus.uni), this, nameof(UpdateHealthBar)));
        //unit.Connect(nameof(BaseUnit.UnitStatusChanged), this, nameof(UpdateStatuses)));
        // Initial update
        UpdateAllInfo(unit);
    }

    private void OnUnitDeselected()
    {
        // Unsubscribe from unit-specific signals
        //unit.Disconnect(nameof(BaseUnit.UnitHealthChanged), this, nameof(UpdateHealthBar));
        //unit.Disconnect(nameof (BaseUnit.UnitStatusChanged), this, nameof(UpdateStatuses));
        // Clear UI elements
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/UnitName").Text = "";
        GetNode<TextureRect>("SingleUnitContainer/VBoxContainer/Portrait").Texture = null;
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/HealthPoints").Text = "";
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/MovementPoints").Text = "";
        // ... etc
        this.Visible = false; // Hide the unit info panel
    }

    private void UpdateAllInfo(BaseUnit unit)
    {
        GD.Print("Updating unit info for: " + unit.UnitName);
        if (unit is ArmyStack)
        {
            _UnitView.Visible = false;
            _groupView.Visible = true;
            GetNode<Label>("GroupContainer/GridContainer/MovementPoints").Text = unit.MovementsLeft.ToString();

            return;
        }

        
        // Update all UI elements
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/UnitName").Text = unit.UnitName;
        GetNode<TextureRect>("SingleUnitContainer/VBoxContainer/Portrait").Texture = unit.Portrait.Texture;
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/HealthPoints").Text = unit.CurrentHealth.ToString();
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/MovementPoints").Text = unit.MovementsLeft.ToString();
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/DefensePoints").Text = unit.Defense.ToString();
        GetNode<TextureRect>("SingleUnitContainer/VBoxContainer/GridContainer/AttackTag").Texture = unit.Data.AttackIcon;
        GetNode<Label>("SingleUnitContainer/VBoxContainer/GridContainer/AttackPoints").Text = unit.Attack.ToString();
        // ... etc
    }

    public override Control _MakeCustomTooltip(string forText)
    {
        var tooltip = ResourceLoader.Load<PackedScene>("res://scenes/UI/UnitTooltipPopup.tscn").Instantiate();

        //tooltip.GetNode<Label>("Label").Text = forText;
        return (PanelContainer) tooltip;
    }
}
