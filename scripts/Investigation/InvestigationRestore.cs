using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Rebuilds runtime investigation state from a loaded save. Clue display text/topics
    /// come from the authored ClueEntry resources (keyed by ClueId); the save only
    /// restores WHICH clues exist and their Marked flag.
    /// </summary>
    public static class InvestigationRestore
    {
        public static Vector2 Apply(
            InvestigationSaveData data,
            Logbook logbook,
            TopicParser parser,
            IReadOnlyDictionary<string, ClueEntry> authoredClues, // ClueId -> resource
            IEnumerable<WitnessState> witnesses,
            List<string> playedEvents)
        {
            if (data == null) return Vector2.Zero;

            foreach (var packet in data.Clues)
            {
                if (!authoredClues.TryGetValue(packet.ClueId, out var authored))
                    continue;

                var entry = logbook.Encounter(authored);
                parser.Learn(entry.Topics);

                if (packet.Marked)
                {
                    logbook.Mark(entry.ClueId);
                    parser.RegisterTag(entry); // re-enable '#tag' quick-ask
                }
            }

            foreach (var w in witnesses)
                if (data.WitnessDisposition.TryGetValue(w.WitnessId, out var d))
                    w.Disposition = d;

            playedEvents.Clear();
            playedEvents.AddRange(data.PlayedEvents);

            return new Vector2(data.CurrentX, data.CurrentY);
        }
    }
}