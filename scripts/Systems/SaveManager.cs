using dot_battle_signal_bus.scripts.Core.Factories;
using dot_battle_signal_bus.scripts.Core.Save;
using dot_battle_signal_bus.scripts.Utils;
using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Systems
{
    /// <summary>
    /// Responsibilities: save to disk, load from disk, serialize/deserialize.
    /// Owns NO game state — it asks the owning systems for their data.
    /// </summary>
    public partial class SaveManager : Node
    {
        private const string SavePath = "user://savegame.json";

        private static SaveManager _instance;
        public static SaveManager Instance => _instance;

        private Node UnitsRoot => GetNode<Node>("/root/Main/GameWorld/Units");
        private GridManager Grid => GetNode<GridManager>("/root/Main/GameWorld/Map");

        // Plain-service factories, keyed by their self-declared category.
        private readonly Dictionary<string, ISaveableFactory> _factories = new();

        [Export] public string BaseUnitScenePath = "res://scenes/BaseUnit.tscn";
        [Export] public string UnitDataFolder = "res://resources/Units/";
        [Export] public string ItemFolder = "res://scenes/Items/";

        public override void _Ready()
        {
            if (_instance == null) _instance = this;
            else { QueueFree(); return; }

            RegisterFactory(new UnitFactory(BaseUnitScenePath, UnitDataFolder));
            RegisterFactory(new ItemFactory(ItemFolder));
        }

        private void RegisterFactory(ISaveableFactory factory) =>
            _factories[factory.Category] = factory;

        // ---------------- SAVE ----------------

        public void SaveGame()
        {
            var scene = new Core.Save.SceneState
            {
                ScenePath = GetTree().CurrentScene.SceneFilePath,
                Objects = BuildObjectArray()
            };

            var data = new Godot.Collections.Dictionary
        {
            { "turnCount", TurnManager.Instance.TurnCount },
            { "state", (int)TurnManager.Instance.CurrentState },
            { "scene", scene.ToDictionary() },
            { "story", StoryManager.Instance.CaptureState().ToDictionary() }
        };

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(Json.Stringify(data));
            GD.Print($"Game saved to {SavePath}");
        }

        private Godot.Collections.Array BuildObjectArray()
        {
            var array = new Godot.Collections.Array();

            foreach (var pair in Grid.GetOccupiedCells())
            {
                if (pair.Key is not ISaveable saveable)
                    continue;

                var entry = saveable.ToSaveData();   // includes its own "category"
                entry["saveId"] = saveable.SaveId;
                entry["gridX"] = pair.Value.X;
                entry["gridY"] = pair.Value.Y;
                array.Add(entry);
            }

            return array;
        }

        // ---------------- LOAD ----------------

        public async void LoadGame()
        {
            if (!FileAccess.FileExists(SavePath))
            {
                GD.PrintErr("No save file found.");
                return;
            }

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            var parsed = Json.ParseString(file.GetAsText());
            if (parsed.VariantType != Variant.Type.Dictionary)
            {
                GD.PrintErr("Save file is corrupt.");
                return;
            }

            var data = parsed.AsGodotDictionary();
            var scene = Core.Save.SceneState.FromDictionary(data["scene"].AsGodotDictionary());

            StoryManager.Instance.RestoreState(
                StoryState.FromDictionary(data["story"].AsGodotDictionary()));

            GetTree().ChangeSceneToFile(scene.ScenePath);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            TurnManager.Instance.LoadSaveData(
                (int)data["turnCount"],
                (TurnManager.GameState)(int)data["state"]);

            Grid.OccupiedCells.Clear();
            foreach (var obj in scene.Objects)
                SpawnObjectFromSave(obj.AsGodotDictionary());

            GD.Print("Game loaded.");
        }

        private void SpawnObjectFromSave(Godot.Collections.Dictionary dict)
        {
            var category = (string)dict["category"];
            var saveId = (string)dict["saveId"];

            if (!_factories.TryGetValue(category, out var factory))
            {
                GD.PrintErr($"SaveManager: no factory for category '{category}'.");
                return;
            }

            var node = factory.Create(saveId);
            if (node is not ISaveable saveable)
            {
                GD.PrintErr($"SaveManager: '{category}/{saveId}' is not ISaveable.");
                return;
            }

            if (node is BaseUnit unit)
            {
                UnitsRoot.AddChild(unit);
                saveable.LoadSaveData(dict);

                var cell = new Vector2I((int)dict["gridX"], (int)dict["gridY"]);
                unit.GlobalPosition = Utils.Utils.GridToWorld(cell, Grid.GetCellSize());
                Grid.RegisterUnitOnCell(cell, unit);
                unit.RefreshAfterLoad();
            }
            else
            {
                GetTree().CurrentScene.AddChild(node);
                saveable.LoadSaveData(dict);
            }
        }
    }
}
