using Godot;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Typed questioning UI (Option A) with '#' quick-ask for Marked clues (Option C).
    /// Wire a LineEdit and an ItemList (suggestions) in the scene.
    /// </summary>
    public partial class QuestioningPanel : Control
    {
        [Export] public NodePath TopicInputPath;
        [Export] public NodePath SuggestionsPath;

        private LineEdit _input;
        private ItemList _suggestions;
        private TopicParser _parser;

        public void Init(TopicParser parser) => _parser = parser;

        public override void _Ready()
        {
            _input = GetNode<LineEdit>(TopicInputPath);
            _suggestions = GetNodeOrNull<ItemList>(SuggestionsPath);

            _input.TextSubmitted += OnSubmit;
            _input.TextChanged += OnTextChanged;
        }

        private void OnTextChanged(string text)
        {
            if (_suggestions == null || !text.StartsWith("#")) return;

            _suggestions.Clear();
            foreach (var tag in _parser.TagSuggestions())
                _suggestions.AddItem("#" + tag);
            _suggestions.Visible = _suggestions.ItemCount > 0;
        }

        private void OnSubmit(string text)
        {
            var topicId = _parser.Resolve(text);
            _input.Clear();
            if (_suggestions != null) _suggestions.Visible = false;

            if (topicId == null)
            {
                // Open, not spoon-fed: no hint, just a dead end.
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.DialogueLineShown,
                    "You don't know enough to ask about that.");
                return;
            }

            SignalBus.Instance.EmitSignal(SignalBus.SignalName.TopicAsked, topicId);
        }
    }
}
