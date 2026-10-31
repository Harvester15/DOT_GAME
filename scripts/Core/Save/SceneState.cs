using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Save
{
    /// <summary>Transient state of the level currently being played. Rebuilt on every load.</summary>
    public class SceneState
    {
        public string ScenePath;                        // the .tscn being played
        public Godot.Collections.Array Objects = new(); // serialized ISaveables on the scene

        public Godot.Collections.Dictionary ToDictionary() => new()
        {
            { "scenePath", ScenePath },
            { "objects", Objects }
        };

        public static SceneState FromDictionary(Godot.Collections.Dictionary d) => new()
        {
            ScenePath = (string)d["scenePath"],
            Objects = d["objects"].AsGodotArray()
        };
    }
}
