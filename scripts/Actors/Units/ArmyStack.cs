using Godot;
using System.Collections.Generic;
using System.Linq;


[GlobalClass]
public partial class ArmyStack : BaseUnit
{
    [Export] public Godot.Collections.Array<PackedScene> StartingUnits;

    public List<BaseUnit> Units = new List<BaseUnit>();
    public int MovementPoints;

    public override void _Ready()
    {
        // If we placed this in the editor and gave it starting units...
        if (StartingUnits != null && StartingUnits.Count > 0)
        {
            foreach (var unitScene in StartingUnits)
            {
                var unitInstance = unitScene.Instantiate<BaseUnit>();
                // We add them as children and register them
                AddUnit(unitInstance);
            }
            InitializeVisuals();
            InputPickable = true;
        }

        // Ensure base logic runs if not already handled
        base._Ready();
    }

    protected override void InitializeCore()
    {
        Name = "ArmyStack";
        return;
    }

    protected override void InitializePlacement()
    {
        GD.Print($"initial placing for {this}");
        _hasBeenPlaced = true;
    }

    protected override void InitializeVisuals()
    {
        if (Units.Count > 0)
        {
            var firstUnit = Units[0];

            TextureRect stackIcon = new TextureRect();
            var unitPortrait = (TextureRect)firstUnit.GetNode<TextureRect>("TextureRect").Duplicate();
            stackIcon = unitPortrait;
            stackIcon.Texture = unitPortrait.Texture;
            AddChild(unitPortrait);
            unitPortrait.Visible = true;
            Portrait = unitPortrait;
            CollisionShape2D stackCol = new CollisionShape2D();
            stackCol = (CollisionShape2D)Units[0].GetNode<CollisionShape2D>("CollisionShape2D").Duplicate();
            AddChild(stackCol);
        }
    }

    

    public ArmyStack CreateUnit(BaseUnit unit1, BaseUnit unit2)
    {

        Units.Add(unit1);
        Units.Add(unit2);

        GD.Print($"Creating ArmyStack with units: {unit1.Name} and {unit2.Name}");
        SwallowUnit(unit1);
        SwallowUnit(unit2);
        GD.Print($"Armystack has a position: {Position} and a global position: {GlobalPosition}");
        Name = $"ArmyStack_{unit1.Name}_{unit2.Name}";
        MovementPoints = Mathf.Min(unit1.MovementRange, unit2.MovementRange);
        MovementsLeft = Mathf.Min(unit1.MovementsLeft, unit2.MovementsLeft);
        InputPickable = true;

        GD.Print($"ArmyStack created with name {this.Name} and movement points {this.MovementPoints}");
        return this;
    }

    //display each unit in the stack with a different hp and movement points,
    //but use the lowest movement points for the whole stack

    public void AddUnit(BaseUnit unit)
    {
        if (!Units.Contains(unit))
            Units.Add(unit);
        // Logic to update stack visuals (e.g., change sprite to leader)
        SwallowUnit(unit);
        UpdateStats();
    }

    private void UpdateStats()
    {
        if (Units.Count == 0) return;
        // Lowest movement range of the whole group
        MovementsLeft = Units.Min(u => u.MovementsLeft);
        MovementRange = Units.Min(u => u.MovementRange);
    }


    private void SwallowUnit(BaseUnit unit)
    {
        unit.IsGrouped = true;
        unit.Visible = false;
        unit.ProcessMode = ProcessModeEnum.Disabled;
        var parent = unit.GetParent();
        if (parent != null)
        {
            parent.RemoveChild(unit);
            if (GetParent() == null)
                parent.AddChild(this);
        }
        AddChild(unit);
        GD.Print($"Added unit as child of ArmyStack. Current children count: {Units.Count}");
    }

    public void RemoveUnit(BaseUnit unit)
    {
        Units.Remove(unit);
        // Logic to update stack visuals if needed
    }
    public new TextureRect GetSprite()
    {
        var sp = base.GetSprite();
        var sprite = Portrait;
        return sprite;
    }


    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        base._InputEvent(viewport, @event, shapeIdx);
    }
}
