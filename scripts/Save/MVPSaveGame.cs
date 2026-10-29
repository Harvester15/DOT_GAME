using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Save
{
    public partial class MVPSaveGame : Node
    {
        private string SavePath => Path.Combine(OS.GetUserDataDir(), "savegame.json");

        public void SaveGame(MVPSaveData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(SavePath, json);
                GD.Print($"Game saved to: {SavePath}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save game: {e.Message}");
            }
        }

        // Load game data from file
        public MVPSaveData LoadGame()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    GD.Print("No save file found.");
                    return null;
                }

                string json = File.ReadAllText(SavePath);
                MVPSaveData data = JsonSerializer.Deserialize<MVPSaveData>(json);
                GD.Print("Game loaded successfully.");
                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load game: {e.Message}");
                return null;
            }
        }

        // Delete save file
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                    GD.Print("Save file deleted.");
                }
                else
                {
                    GD.Print("No save file to delete.");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to delete save: {e.Message}");
            }
        }
    }
}
