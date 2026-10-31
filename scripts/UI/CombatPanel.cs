using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Combat;
using dot_battle_signal_bus.scripts.Combat.Systems;
using dot_battle_signal_bus.scripts.Systems;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CombatPanel : Panel
{
    private CombatManager _combatManager => GetNode<CombatManager>("/root/Main/GameWorld/Managers/CombatManager");
    private RichTextLabel _combatLabel => GetNode<RichTextLabel>("Homam/CombatLabel");
    private Label _critHitLabel => GetNode<Label>("Homam/CritialHitRect");
    private GridContainer _attackerGrid => GetNode<GridContainer>("Homam/AttackGridContainer");
    private GridContainer _defenderGrid => GetNode<GridContainer>("Homam/DefendGridContainer");
    private TextureRect _critHitRect => GetNode<TextureRect>("Homam/CritialHitRect");

    private Node2D _homam => GetNode<Node2D>("Homam");
    private Node2D _darkest => GetNode<Node2D>("Darkest");

    private OptionButton _combatModeSelector => GetNode<OptionButton>("CombatModeSelector");

    private System.Collections.Generic.Dictionary<string, Texture2D> _iconKeys = new System.Collections.Generic.Dictionary<string, Texture2D>();
    private BaseUnit _attacker;
    private BaseUnit _defender;

    public override void _Ready()
    {
        Visible = false; // Hide the combat panel by default
        // Connect to global signals
        SignalBus.Instance.UnitAttacked += OnUnitAttacked;
        //SignalBus.Instance.CombatModeSelected += OnCombatMode;
        //SignalBus.Instance.CombatModeSelected += OnCombatStarted;
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
            _defenderGrid.AddChild(combatSlot);
        }
        _combatLabel.Text = $"Combat Started: {player.Count} player units vs {enemy.Count} enemy units";

    }

    public void _on_combat_mode_selected (int index)
    {
        //TODO implement combat mode selection (e.g. normal, aggressive, defensive, etc.)
        GD.Print($"Combat mode selected: {index}");
        switch (index)
        {
            case 0: _combatManager.SetCombatSystem(new WarlordsCombatSystem()); break;
            case 1: _combatManager.SetCombatSystem(new DarkestCombatSystem()); break;
            case 2: _combatManager.SetCombatSystem(new HommCombatSystem()); break;
        }
        AddChild(_combatManager.combatSystem as Node);
    }

    public void _on_button_pressed()
    {
        GD.Print($"Combat button pressed, starting combat with selected combat system: {_combatManager.combatSystem.GetType().Name}");
        if (_combatManager.combatSystem is GodotObject godotSystem)
        {
            SignalBus.Instance.EmitSignal("CombatModeSelected", _attacker, _defender, godotSystem);
        }
        _combatModeSelector.Visible = false;
    }

    private void OnUnitAttacked(BaseUnit attacker, BaseUnit target)
    {
        GD.Print($"CombatPanel received CombatStarted signal: {attacker} player units vs {target} enemy units");
        Visible = true;

        _combatModeSelector.Visible = true;

        _attacker = attacker;
        _defender = target;

    }
}
