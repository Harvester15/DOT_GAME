using Godot;

public partial class HoverTooltipComponent : TextureRect
{
    private BaseUnit _trackedUnit;
    private PackedScene _popupScene = GD.Load<PackedScene>("res://scenes/UI/UnitTooltipPopup.tscn");

    public void Initialize(BaseUnit unit)
    {
        _trackedUnit = unit;
        // This triggers Godot's tooltip system to call _MakeCustomTooltip
        TooltipText = " ";
    }

    public override Control _MakeCustomTooltip(string forText)
    {
        // Instancing your popup UI on demand when hovered
        var popup = _popupScene.Instantiate<UnitTooltipPopup>();
        popup.Setup(_trackedUnit);
        return popup;
    }
}