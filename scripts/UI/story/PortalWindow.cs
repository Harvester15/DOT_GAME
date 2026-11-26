using dot_battle_signal_bus.scripts.Investigation;
using dot_battle_signal_bus.scripts.Systems;
using Godot;
using System;
using System.Text.RegularExpressions;

public partial class PortalWindow : Control
{

    private InvestigationParser Parser => StoryManager.Instance.Parser;
    
    private RichTextLabel KnownLocations => GetNode<RichTextLabel>("Panel/KnownLocations");
    private RichTextLabel Description => GetNode<RichTextLabel>("Panel/Description");
    private Label LocationName => GetNode<Label>("Panel/LocationName");
    private TextureRect LocationIcon => GetNode<TextureRect>("Panel/LocationIcon");
    private LocationData _selectedLocation;

    private void OnTravelPressed()
    {
        if (_selectedLocation == null)
        {
            return;
        }
        LocationManager.CurrentLocation =
            _selectedLocation;

        GetTree()
            .ChangeSceneToFile(
                "res://Scenes/LocationScene.tscn");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        KnownLocations.MetaClicked += OnLocationSelected;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


    private void OnLocationSelected(
    Variant meta)
    {
        string id = meta.AsString();

        if (!StoryManager.Instance
            .Locations.TryGetValue(id,
                out var location))
        {
            return;
        }

        _selectedLocation = location;

        LocationName.Text =
            location.Name;

        Description.Text =
            location.Description;

        LocationIcon.Texture =
            location.Icon;
    }


    private void ShowLocations()
    {
        KnownLocations.Clear();

        foreach (var locationToken in Parser.Locations)
        {
            string id = locationToken.TrimStart('@');

            AppendEntry(id);
        }
    }

    private void ShowDetails()
    {
        Description.Clear();

        string key;
        foreach (var description in Parser.Locations)
        {
            key = CleanStringFromTags(description);
            AppendEntry("@" + key);
        }
    }

    private void AppendEntry(string id)
    {
        if (!StoryManager.Instance
            .Locations.TryGetValue(id,
            out var location))
        {
            return;
        }

        KnownLocations.AppendText(
            $"[url={id}]"
          + $"{location.Name}"
          + "[/url]\n");
    }

    private string CleanStringFromTags(string input)
    {
        return Regex.Replace(input, "[#@!]", "");
    }
}
