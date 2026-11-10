using dot_battle_signal_bus.scripts.Investigation;
using dot_battle_signal_bus.scripts.Investigation.Services;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace dot_battle_signal_bus.scripts.UI
{

    public partial class ArchiveOfEchoes : Control
    {
        private Panel _journalPanel;

        private Control _witnessHeader;
        private TextureRect _background;
        private TextureRect _witnessPortrait;
        private Label _witnessName;

        private RichTextLabel _statementLabel;
        private LineEdit _responseLineEdit;

        private ConversationService _conversationService = new();



        public override void _Ready()
        {
            _journalPanel = GetNode<Panel>("JournalPanel");

            _witnessHeader = GetNode<Control>(
                "VBoxContainer/WitnessHeader");

            _background = GetNode<TextureRect>(
                "VBoxContainer/WitnessHeader/Background");

            _witnessPortrait = GetNode<TextureRect>(
                "VBoxContainer/WitnessHeader/CenterContainer/VBoxContainer/Witness");

            _witnessName = GetNode<Label>(
                "VBoxContainer/WitnessHeader/CenterContainer/VBoxContainer/WitnessName");

            _statementLabel = GetNode<RichTextLabel>(
                "VBoxContainer/VBoxContainer/HBoxContainer/WitnessRichTextLabel");

            _responseLineEdit = GetNode<LineEdit>(
                "VBoxContainer/VBoxContainer/ResponseLineEdit");

            _statementLabel.SelectionEnabled = true;

            _conversationService.Initialize();
            
            Visible = false;

            SignalBus.Instance.DisplayWitness += ShowWitness;
            SignalBus.Instance.DialogueLineShown += DisplayDialogue;
        }

        private void DisplayDialogue(string text)
        {
            _statementLabel.Text = text;
        }

        public void ShowWitness(WitnessData witness)
        {
            _witnessName.Text = witness.DisplayName;
            _witnessPortrait.Texture = witness.Portrait;
            _background.Texture = witness.Background;
            _statementLabel.Text = witness.IntroStatement;

            _conversationService.BeginConversation(witness, new WitnessState() {WitnessId = witness.DisplayName });
            Toggle();
        }



        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("archive"))
            {
                GD.Print("archive from ArchiveOfEchoes");
                Toggle();
                //GetViewport().SetInputAsHandled();
            }
            
        }


        public void Open()
        {
            Visible = true;
            GetTree().Paused = true;

            ProcessMode = ProcessModeEnum.Always;

            _responseLineEdit?.GrabFocus();
        }

        public void Close()
        {
            Visible = false;

            GetTree().Paused = false;
        }

        public void Toggle()
        {
            Visible = !Visible;

            if (Visible)
                _responseLineEdit?.GrabFocus();
        }

        public bool IsOpen => Visible;
    }
}
