using dot_battle_signal_bus.scripts.Investigation;
using dot_battle_signal_bus.scripts.Investigation.Services;
using dot_battle_signal_bus.scripts.Systems;
using dot_battle_signal_bus.scripts.UI;
using Godot;
using System;

public partial class Locations : Control
{
    private Panel _journalPanel;

    private Control _witnessHeader;
    private TextureRect _background;
    private TextureRect _witnessPortrait;
    private Label _witnessName;

    private RichTextLabel _statementLabel;
    private LineEdit _responseLineEdit;

    private ConversationService _conversationService = new();

    [Export]
    private NotebookWindow _notebookWindow;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var location =
            LocationManager.CurrentLocation;

        LoadLocation(location);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    private void LoadLocation(
    LocationData location)
    {
        _witnessName.Text =
            location.Witness.DisplayName;

        _responseLineEdit.Text =
            location.Description;
    }
}
