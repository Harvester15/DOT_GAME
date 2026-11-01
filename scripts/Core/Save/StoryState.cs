using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Save
{
    /// <summary>Persistent progress that survives across scenes (flags, clues, etc.).</summary>
    public class StoryState
    {
        public Godot.Collections.Dictionary Flags = new();  // bool/int progress switches
        public Godot.Collections.Dictionary Variables = new();  // named numeric/string values
        public Godot.Collections.Array Evidence = new();  // collected evidence ids
        public Godot.Collections.Array Witnesses = new();  // encountered witness ids
        public Godot.Collections.Array Notes = new();  // player/journal notes

        public Godot.Collections.Dictionary ToDictionary() => new()
        {
            { "flags", Flags },
            { "variables", Variables },
            { "evidence", Evidence },
            { "witnesses", Witnesses },
            { "notes", Notes }
        };

        public static StoryState FromDictionary(Godot.Collections.Dictionary d) => new()
        {
            Flags = d["flags"].AsGodotDictionary(),
            Variables = d["variables"].AsGodotDictionary(),
            Evidence = d["evidence"].AsGodotArray(),
            Witnesses = d["witnesses"].AsGodotArray(),
            Notes = d["notes"].AsGodotArray()
        };
    }
}
