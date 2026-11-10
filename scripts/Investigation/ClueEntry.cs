using Godot;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public enum ClueKind { Coord, Name, Item, Statement, Contradiction }

    /// <summary>
    /// A single piece of evidence, authorable in the Godot Inspector like UnitData.
    /// Surfaced by dialogue as "encountered" (seen); only persisted if the player Marks it.
    /// </summary>
    [GlobalClass]
    public partial class ClueEntry : Resource
    {
        /// <summary>
        /// the actual key that will be saved in the notebook and retrieved by #
        /// </summary>
        [Export] public string ClueId;            // stable key, e.g. "outbound_logs"
        [Export] public ClueKind Kind;
        [Export(PropertyHint.MultilineText)] public string DisplayText;
        [Export] public string SourceWitness;     // who revealed it (bias tracking)
        [Export] public string EventId;           // optional: related shared event

        [Export] public Vector2 Coord;            // portal destination (Kind == Coord)

        // Words that let the TopicParser match this clue when typed.
        [Export] public string[] Topics = System.Array.Empty<string>();

        // Runtime only (not authored): player's keep state.
        public bool Marked;
    }
}