using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>Serializable snapshot of investigation progress.</summary>
    public class InvestigationSaveData
    {
        public List<CluePacket> Clues { get; set; } = new();
        public Dictionary<string, int> WitnessDisposition { get; set; } = new();
        public List<string> PlayedEvents { get; set; } = new();
        public float CurrentX { get; set; }
        public float CurrentY { get; set; }

        public class CluePacket
        {
            public string ClueId { get; set; }
            public bool Marked { get; set; }
            public int Kind { get; set; }
            public float CoordX { get; set; }
            public float CoordY { get; set; }
        }
    }

    public static class InvestigationSave
    {
        private const string Path = "user://investigation.json";

        public static void Save(Logbook logbook, IEnumerable<WitnessState> witnesses,
                                IEnumerable<string> playedEvents, Vector2 location)
        {
            var data = new InvestigationSaveData
            {
                Clues = logbook.Encountered().Select(c => new InvestigationSaveData.CluePacket
                {
                    ClueId = c.ClueId,
                    Marked = c.Marked,
                    Kind = (int)c.Kind,
                    CoordX = c.Coord.X,
                    CoordY = c.Coord.Y
                }).ToList(),
                WitnessDisposition = witnesses.ToDictionary(w => w.WitnessId, w => w.Disposition),
                PlayedEvents = playedEvents.ToList(),
                CurrentX = location.X,
                CurrentY = location.Y
            };

            var json = JsonSerializer.Serialize(data);
            using var file = FileAccess.Open(Path, FileAccess.ModeFlags.Write);
            file.StoreString(json);
        }

        public static InvestigationSaveData Load()
        {
            if (!FileAccess.FileExists(Path)) return null;
            using var file = FileAccess.Open(Path, FileAccess.ModeFlags.Read);
            return JsonSerializer.Deserialize<InvestigationSaveData>(file.GetAsText());
        }
    }
}
