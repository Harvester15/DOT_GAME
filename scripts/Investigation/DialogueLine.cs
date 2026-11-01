using Godot;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// One authored witness response, editable in the Inspector.
    /// Carries an optional clue to surface and optional [TELL]/[BRANCH] metadata.
    /// </summary>
    [GlobalClass]
    public partial class DialogueLine : Resource
    {
        [Export(PropertyHint.MultilineText)] public string Text;
        [Export] public ClueEntry Clue;      // null = pure flavor, nothing to Mark
        [Export] public string[] Tells;      // [TELL] cues; never auto-interpreted
        [Export] public string BranchId;     // [BRANCH:x] optional path unlocked

        // Which typed topic triggers this line (parser resolves input to this id).
        [Export] public string TopicId;
    }
}