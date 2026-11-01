using Godot;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Plays authored DialogueLine resources. When a line carries a clue, it emits a
    /// signal so the UI can offer Mark / Read. Nothing is logged automatically.
    /// </summary>
    public partial class DialogueRunner : Node
    {
        private readonly Logbook _logbook;
        private readonly TopicParser _parser;

        public DialogueRunner(Logbook logbook, TopicParser parser)
        {
            _logbook = logbook;
            _parser = parser;
        }

        public void Deliver(DialogueLine line)
        {
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.DialogueLineShown, line.Text);

            if (line.Clue != null)
            {
                var entry = _logbook.Encounter(line.Clue);
                _parser.Learn(entry.Topics);
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.ClueEncountered, entry.ClueId, entry.DisplayText);
            }
        }

        public void MarkClue(string clueId)
        {
            _logbook.Mark(clueId);
            var clue = _logbook.Get(clueId);
            if (clue != null)
            {
                _parser.RegisterTag(clue);
                SignalBus.Instance.EmitSignal(SignalBus.SignalName.ClueMarked, clueId);
            }
        }
    }
}