using Godot;
using System.Security.Cryptography.X509Certificates;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Typed questioning UI (Option A) with '#' quick-ask for Marked clues (Option C).
    /// Wire a LineEdit and an ItemList (suggestions) in the scene.
    /// </summary>
    public partial class QuestioningPanel : LineEdit
    {
        [Export] public NodePath TopicInputPath;
        [Export] public NodePath SuggestionsPath;
        private RichTextLabel _statementLabel;
        private TextureButton _noteBook;
        private LineEdit _input;
        private ItemList _suggestions;

        private TopicParser _parser;
        private string _highlighted = string.Empty;

        public void Init(TopicParser parser) => _parser = parser;


        public override void _Ready()
        {

            _statementLabel = GetNode<RichTextLabel>(
                "../HBoxContainer/WitnessRichTextLabel");

            _noteBook = GetNode<TextureButton>(
                "../HBoxContainer/NoteBookBtn");
            _noteBook.Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);

            _input = GetNode<LineEdit>(TopicInputPath);
            _suggestions = GetNodeOrNull<ItemList>(SuggestionsPath);

            _input.TextSubmitted += OnSubmit;

            if (_parser == null)
                Init(new TopicParser());

            _input.TextChanged += OnTextChanged;


            _suggestions= GetNode<ItemList>("../../../ItemList");

            // Click a suggestion to accept it.
            if (_suggestions != null)
                _suggestions.ItemActivated += OnSuggestionActivated;   // double-click / Enter
            if (_suggestions != null)
                _suggestions.ItemSelected += OnSuggestionActivated;    // single click

            if (_suggestions != null)
                _suggestions.Visible = false;


        }


        private void OnTextChanged(string text)
        {
            // Only autocomplete when the player is typing a '#tag'.
            int hashIndex = text.LastIndexOf('#');
            if (_suggestions == null || hashIndex < 0)
            {
                HideSuggestions();
                return;
            }
            _suggestions.GlobalPosition = new Vector2(_input.GlobalPosition.X, _input.GlobalPosition.Y + _input.Size.Y / 2);
            // The partial text right after the '#'.
            string fragment = text.Substring(hashIndex + 1).ToLowerInvariant();

            _suggestions.Clear();
            foreach (var tag in _parser.TagSuggestions())
            {
                if (fragment.Length == 0 || tag.ToLowerInvariant().Contains(fragment))
                    _suggestions.AddItem("#" + tag);
            }

            _suggestions.Visible = _suggestions.ItemCount > 0;
        }

        private void OnSuggestionActivated(long index)
        {
            if (_suggestions == null) return;

            string chosen = _suggestions.GetItemText((int)index); // e.g. "#outbound_logs"

            // Replace the partial '#fragment' in the input with the full tag.
            int hashIndex = _input.Text.LastIndexOf('#');
            if (hashIndex >= 0)
                _input.Text = _input.Text.Substring(0, hashIndex) + chosen + " ";

            HideSuggestions();
            _input.GrabFocus();
            _input.CaretColumn = _input.Text.Length; // move caret to end
        }

        private void HideSuggestions()
        {
            if (_suggestions == null) return;
            _suggestions.Clear();
            _suggestions.Visible = false;
        }



        private void OnSubmit(string text)
        {
            string topicId = text;

            SignalBus.Instance.EmitSignal(SignalBus.SignalName.TopicAsked, topicId);
            _input.Clear();
        }

        public void _on_note_book_btn_mouse_entered()
        {
            if (_statementLabel.GetSelectedText() != "")
            {
                _highlighted = _statementLabel.GetSelectedText();
            }
        }

        public void _on_note_book_btn_gui_input(InputEvent @event)
        {

            

            if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == MouseButton.Left)
            {
                if (_highlighted != "")
                {
                    GD.Print($"Selected text: {_statementLabel.GetSelectedText()} beind saved to notebook");
                    _parser.RegisterTag(new ClueEntry
                    {
                        ClueId = _highlighted,
                        DisplayText = _highlighted,
                        Topics = new[] { _highlighted }
                    });
                    _highlighted = "";
                    _noteBook.Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
                    SignalBus.Instance.EmitSignal(SignalBus.SignalName.ClueMarked, _highlighted);
                }
                else
                {
                    GD.Print("No text selected. for notebook");
                    GD.Print($"highlighted text: {_highlighted}");
                }
            }
        }


        public void _on_witness_rich_text_label_gui_input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (_statementLabel.GetSelectedText() != "")
                {
                    var selectedText = _statementLabel.GetSelectedText();
                    GD.Print($"Selected text from label: {selectedText}");
                    _highlighted = selectedText;
                    _noteBook.Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1);
                }
                else
                {
                    GD.Print("No text selected.");
                    GD.Print($"highlighted text: {_statementLabel.GetSelectedText()}");
                    _noteBook.Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
                }
            }

        }

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            if (!_suggestions.Visible)
                return;

            if (@event is not InputEventKey key )
                return;

            switch (key.Keycode)
            {
                case Key.Down:
                    MoveSuggestion(1);
                    GetViewport().SetInputAsHandled();
                    break;

                case Key.Up:
                    MoveSuggestion(-1);
                    GetViewport().SetInputAsHandled();
                    break;

                case Key.Tab:
                    AcceptCurrentSuggestion();
                    GetViewport().SetInputAsHandled();
                    break;

                case Key.Enter:
                    if (_suggestions.Visible)
                    {
                        AcceptCurrentSuggestion();
                        GetViewport().SetInputAsHandled();
                    }
                    break;
            }
        }

        private void MoveSuggestion(int direction)
        {
            if (_suggestions.ItemCount == 0)
                return;

            int current = 0;

            var selected = _suggestions.GetSelectedItems();

            if (selected.Length > 0)
                current = selected[0];

            current += direction;

            current = Mathf.Clamp(
                current,
                0,
                _suggestions.ItemCount - 1);

            _suggestions.DeselectAll();
            _suggestions.Select(current);
            _suggestions.EnsureCurrentIsVisible();
        }

        private void AcceptCurrentSuggestion()
        {
            if (_suggestions == null || !_suggestions.Visible)
                return;

            var selected = _suggestions.GetSelectedItems();

            if (selected.Length == 0)
            {
                if (_suggestions.ItemCount > 0)
                    OnSuggestionActivated(0);

                return;
            }

            OnSuggestionActivated(selected[0]);
        }
    }
}
