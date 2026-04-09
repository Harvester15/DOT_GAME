using dot_battle_signal_bus.scripts;
using Godot;
using System;
using System.Collections.Generic;

public partial class TurnIcon : Panel
{
    [ExportGroup("Team Sprites")]
    [Export] public Texture2D NeutralSprite;
    [Export] public Texture2D PlayerSprite;
    [Export] public Texture2D EnemySprite;
    [Export] public Texture2D AllySprite;
    private TextureRect _icon => GetNode<TextureRect>("TurnIcon");
    private Label _turnLabel => GetNode<Label>("Label");
    private Label _turnNumberLabel => GetNode<Label>("TurnNumberLabel");

    public override void _Ready()
    {

        // Connect to the Autoload signals
        TurnManager.Instance.StateChanged += OnStateChanged;
        TurnManager.Instance.TurnChanged += OnTurnChanged;
    }

    private void OnTurnChanged(int turnNumber)
    {
        _turnNumberLabel.Text = $"Turn {turnNumber}";
    }

    private void OnStateChanged(TurnManager.GameState newState)
    {
        _turnLabel.Text = newState.ToString();
        _icon.Texture = newState == TurnManager.GameState.PlayerTurn ? PlayerSprite : EnemySprite;
    }

}
