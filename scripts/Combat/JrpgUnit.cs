using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Combat.Logic;
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BaseGameObject;

public partial class JrpgUnit : Node2D
{

    public Sprite2D UnitSprite => GetNode<Sprite2D>("unit");

    public BaseUnit UnitData { get; set; }
    private Area2D _area => GetNode<Area2D>("Area2D");
    public ProgressBar HealthBar => GetNode<ProgressBar>("HealthBar");
    private Node2D ShieldContainer => GetNode<Node2D>("ShieldContainer");

    [Export] public ShieldData ShieldData { get; set; }

    private Sprite2D QMark => GetNode<Sprite2D>("ShieldContainer/Qmark");
    private Sprite2D QMark2 => GetNode<Sprite2D>("ShieldContainer/Qmark2");
    private Sprite2D QMark3 => GetNode<Sprite2D>("ShieldContainer/Qmark3");
    private List<Sprite2D> QMarks => new List<Sprite2D> { QMark, QMark2, QMark3 };
    public override void _Ready()
    {
        if (UnitData.UnitTeam == Team.Player)
        {
            ShieldContainer.Visible = false;
        }
        else
        {
            ShieldContainer.Visible = true;
        }

        GD.Print("jrpg_unit ready");


        SignalBus.Instance.ShieldHit += ShieldHit;
    }

    private void  ShieldHit(BaseUnit defender, BaseUnit attacker, ShieldData shieldData)
    {

        if (defender == UnitData)
        {
            GD.Print($"Unit '{UnitData.Name}' was hit while shielded! Shield element: {shieldData.Element}, weaknesses: {string.Join(", ", shieldData.Weaknesses)}");
            GD.Print($"Unit, USING OWN SHIELDDATA '{UnitData.Name}' was hit while shielded! Shield element: {ShieldData.Element}, weaknesses: {string.Join(", ", ShieldData.Weaknesses)}");
            // You can add visual effects here based on the shield type, e.g.:
            int qmarkIndex = shieldData.Weaknesses.IndexOf(attacker.Data.Dmg.dmgType);
            var qmark = QMarks[qmarkIndex];
            qmark.Texture = attacker.Data.AttackIcon;

        }
    }

    public void _on_area_2d_mouse_entered()
    {

        UnitSprite.Modulate = new Color(1, 1, 1, 0.5f); // Make the sprite semi-transparent
        SignalBus.Instance.EmitSignal(SignalBus.SignalName.JrpgUnitSelected, this);
    }

    public void _on_area_2d_mouse_exited()
    {
        UnitSprite.Modulate = new Color(1, 1, 1, 1); // Restore the sprite's original color
    }

    // Inside JrpgUnit.cs
    public async Task PlayAttackAnimation(Vector2 targetPosition)
    {
        var startPos = Position;
        // Lunge 40% of the way to the target
        var lungePos = startPos.Lerp(targetPosition, 0.4f);

        Tween tween = GetTree().CreateTween();
        // Move forward fast
        tween.TweenProperty(this, "position", lungePos, 0.45f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        // Stay there for a tiny moment (the "hit")
        tween.TweenInterval(0.05f);
        // Move back
        tween.TweenProperty(this, "position", startPos, 0.85f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task PlayHitReaction()
    {
        var startPos = Position;
        // Bump slightly back based on side
        float direction = UnitSprite.FlipH ? 1.0f : -1.0f;
        var recoilPos = startPos + new Vector2(30 * direction, 0);

        Tween tween = GetTree().CreateTween();
        // Flash Red and Shake back
        UnitSprite.SelfModulate = Colors.Red;
        tween.TweenProperty(this, "position", recoilPos, 0.3f);
        tween.TweenProperty(this, "position", startPos, 0.5f);

        await ToSignal(tween, Tween.SignalName.Finished);
        UnitSprite.SelfModulate = Colors.White;
    }

    public async Task PlayDeathAnimation()
    {
        Tween tween = GetTree().CreateTween();

        // Sink into ground while fading out
        tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.8f);
        tween.Parallel().TweenProperty(this, "position:y", Position.Y + 40, 0.8f);

        // Shake slightly as they "collapse"
        tween.Parallel().TweenProperty(UnitSprite, "rotation", Mathf.DegToRad(15), 0.2f);

        await ToSignal(tween, Tween.SignalName.Finished);
        QueueFree(); // Remove from scene
    }

    public void SpawnPopUp(string text, Color color)
    {
        GD.Print($"Spawning pop-up: '{text}' with color {color}");
        // Simple way: Create a Label, animate it upwards, then delete it
        Label label = new Label();
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Center;

        // Style the label (you can use a LabelSettings resource for better looks)
        label.Modulate = color;
        label.ZIndex = 10;

        label.Scale = new Vector2(2, 2);
        label.TopLevel = true;
        AddChild(label);
        label.GlobalPosition = GlobalPosition + new Vector2(-20, -55);

        GD.Print($"Label created at position {label.Position} - global {label.GlobalPosition}, for unit at position '{Position}' - global {GlobalPosition}");

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(label, "position:y", label.GlobalPosition.Y - 90, 2.5f);
        tween.Parallel().TweenProperty(label, "modulate:a", 0.0f, 0.85f);
        tween.SetTrans(Tween.TransitionType.Circ).SetEase(Tween.EaseType.Out);

        tween.Finished += () => label.QueueFree();
    }

    private void _on_area_2d_input_event(Node viewport, InputEvent @event, int shape_idx)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed && mouseBtn.ButtonIndex == MouseButton.Left)
        {
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.JrpgUnitClickeded, this);
        }
    }

    

}
