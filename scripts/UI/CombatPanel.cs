using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Combat;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CombatPanel : Panel
{
    private RichTextLabel _combatLabel => GetNode<RichTextLabel>("CombatLabel");
    private Label _critHitLabel => GetNode<Label>("CritialHitRect");
    private GridContainer _attackerGrid => GetNode<GridContainer>("AttackGridContainer");
    private GridContainer _defenderGrid => GetNode<GridContainer>("DefendGridContainer");
    private TextureRect _critHitRect => GetNode<TextureRect>("CritialHitRect");

    private System.Collections.Generic.Dictionary<string, Texture2D> _iconKeys = new System.Collections.Generic.Dictionary<string, Texture2D>();

    public override void _Ready()
    {
        Visible = false; // Hide the combat panel by default
        // Connect to global signals
        SignalBus.Instance.UnitAttacked += OnUnitAttacked;
        SignalBus.Instance.CombatStarted += OnCombatStarted;
        SignalBus.Instance.CombatEnded += OnCombatEnded;
        SignalBus.Instance.CriticalHit += OnCriticalHit;
        SignalBus.Instance.CombatLog += OnCombatLog;
        SignalBus.Instance.UnitDied += OnUnitDied;
        SignalBus.Instance.BattleReportClosed += OnBattleReportClosed;
        _combatLabel.BbcodeEnabled = true;
        _combatLabel.Text = "Starting combat";
    }

    private void OnUnitDied(BaseUnit unit)
    {
        GD.Print($"CombatPanel received UnitDied signal: {unit.Name}");
        foreach (CombatSlot slot in _attackerGrid.GetChildren())
        {
            if (slot.Name == unit.Name)
            {
                slot.Portrait.Visible = false;
                _attackerGrid.RemoveChild(slot);
                return;
            }
        }
        foreach (CombatSlot slot in _defenderGrid.GetChildren())
        {
            if (slot.Name == unit.Name)
            {
                slot.Portrait.Visible = false;
                _defenderGrid.RemoveChild(slot);
                return;
            }
        }
    }

    private void OnCombatLog(string combatReport, CombatSlot attacker, CombatSlot defender)
    {
       // UpdateIconKeys(attacker);
       // UpdateIconKeys(defender);

        GD.Print($"CombatPanel received CombatLog signal: {combatReport}");
        _combatLabel.AppendText ( combatReport);
        _combatLabel.ScrollToLine(_combatLabel.GetLineCount());

    }

    private void UpdateIconKeys(CombatSlot unit)
    {
        var key = unit.UnitName + "_icon";
        var texture = unit.Portrait.Texture;
        GD.Print($"Updating icon keys for unit: {unit.UnitName}, key: {key}");
        if (!_iconKeys.ContainsKey(key))
        {
            _iconKeys.Add(key, texture);
            _combatLabel.AddImage(texture, 24, 24, new Color(1,1,1),InlineAlignment.Center,new Rect2(),key);
            _combatLabel.AppendText($"adding unit:  [img={key}]  after image");
        }

    }

    private void OnCriticalHit(BaseUnit attacker, BaseUnit target, int damage)
    {
        _critHitRect.Visible = true;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(_critHitRect, "modulate:a", 1.0f, 1.0f).SetEase(Tween.EaseType.InOut);
        _critHitRect.Visible = false;
    }

    private void OnCombatEnded(CombatResult combatResult)
    {
        if (GetNode<BattleReport>("/root/Main/BattleReport") == null)
        {
            GD.Print($"CombatPanel received CombatEnded signal: {combatResult}");
            var reportScene = GD.Load<PackedScene>("res://scenes/UI/battle_report.tscn");
            var reportInstance = reportScene.Instantiate<BattleReport>();
            GetTree().CurrentScene.AddChild(reportInstance);
            reportInstance.Display(combatResult);

        }

       // SignalBus.Instance.EmitSignal("ShowCombatReport", combatResult);
        
    }

    public void OnBattleReportClosed()
    {

        _combatLabel.Clear();
        ClearGrids();
        Visible = false;
    }
    private void ClearGrids()
    {
        GD.Print("clearing grids in COMBATPANEL");

        var children = _attackerGrid.GetChildren();
        
        foreach (var child in children)
        { _attackerGrid.RemoveChild(child); }

        children = _defenderGrid.GetChildren();
        foreach (var child in children)
        { _defenderGrid.RemoveChild(child); }

    }

    private void OnCombatStarted(Array<BaseUnit> player, Array<BaseUnit> enemy)
    {
        GD.Print($"CombatPanel received CombatStarted signal: {player.Count} player units vs {enemy.Count} enemy units");
        Visible = true;

        foreach (var unit in player)
        {
            CombatSlot combatSlot = new CombatSlot
            {
                Portrait = unit.Portrait.Duplicate() as TextureRect,
                UnitTeam = unit.UnitTeam,
                UnitName = unit.Name
            };

            combatSlot.Portrait.Visible = true;
            var bar = combatSlot.Portrait.GetChildren();
            GD.Print($"getting the potrait children {bar}, # {bar.Count}");
            combatSlot.Portrait.RemoveChild(bar[0]);
            //TextureRect textureRect = unit.Portrait.Duplicate() as TextureRect;
            //textureRect.CustomMinimumSize = new Vector2(50, 50);
            //textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
            //textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
            //textureRect.GetNode<ProgressBar>("HealthBar").Visible = false;
            _attackerGrid.AddChild(combatSlot);

            if (combatSlot.GetChildCount() == 0)
            {
                combatSlot.AddChild(combatSlot.Portrait);
            }
        }

        foreach (var unit in enemy)
        {
            CombatSlot combatSlot = new CombatSlot
            {
                Portrait = unit.Portrait.Duplicate() as TextureRect,
                UnitTeam = unit.UnitTeam,
                UnitName = unit.Name
            };
            combatSlot.Portrait.Visible = true;
            var bar = combatSlot.Portrait.GetChildren();
            GD.Print($"getting the potrait children {bar}, # {bar.Count}");
            combatSlot.Portrait.RemoveChild(bar[0]);
            //TextureRect textureRect = unit.Portrait.Duplicate() as TextureRect;
            //textureRect.CustomMinimumSize = new Vector2(50, 50);
            //textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
            //textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
            //textureRect.GetNode<ProgressBar>("HealthBar").Visible = false;
            _defenderGrid.AddChild(combatSlot);
        }
        _combatLabel.Text = $"Combat Started: {player.Count} player units vs {enemy.Count} enemy units";

    }

    private void OnUnitAttacked(BaseUnit attacker, BaseUnit target)
    {
        //find out who is the attacker and who is the defender, and update the combat panel accordingly
        //foreach (TextureRect textureRect in _attackerGrid.GetChildren())
        //{
        //    if (textureRect.Texture == attacker.Portrait.Texture)
        //    {
        //        textureRect.GetNode<ProgressBar>("HealthBar").Visible = true;
        //        textureRect.GetNode<ProgressBar>("HealthBar").Value = attacker.CurrentHealth;
        //    }
        //}
    }
}
