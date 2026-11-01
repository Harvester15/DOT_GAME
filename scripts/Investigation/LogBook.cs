using System.Collections.Generic;
using System.Linq;

namespace dot_battle_signal_bus.scripts.Investigation
{

    /// <summary>
    /// Single source of truth for discovered evidence. Nothing auto-persists:
    /// Encounter() records a clue as "seen", Mark() is the player's deliberate keep.
    /// Marked clues drive fast-travel, leverage, and the final accusation.
    /// </summary>
    public class Logbook
    {
        private readonly Dictionary<string, ClueEntry> _entries = new();

        /// <summary>Clue has been seen in dialogue but not yet kept (Read).</summary>
        public ClueEntry Encounter(ClueEntry clue)
        {
            if (!_entries.ContainsKey(clue.ClueId))
                _entries[clue.ClueId] = clue; // Marked stays false
            return _entries[clue.ClueId];
        }

        /// <summary>Player deliberately keeps a clue in the Logbook.</summary>
        public void Mark(string clueId)
        {
            if (_entries.TryGetValue(clueId, out var c))
                c.Marked = true;
        }

        public bool HasEncountered(string clueId) => _entries.ContainsKey(clueId);

        public bool HasMarked(string clueId) =>
            _entries.TryGetValue(clueId, out var c) && c.Marked;

        public ClueEntry Get(string clueId) =>
            _entries.TryGetValue(clueId, out var c) ? c : null;

        public IEnumerable<ClueEntry> Encountered() => _entries.Values;

        public IEnumerable<ClueEntry> Marked() => _entries.Values.Where(e => e.Marked);

        /// <summary>Fast-travel list = marked coordinate clues only.</summary>
        public IEnumerable<ClueEntry> KnownPortals() =>
            Marked().Where(e => e.Kind == ClueKind.Coord);
    }
}
