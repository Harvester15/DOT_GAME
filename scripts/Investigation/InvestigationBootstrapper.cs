using dot_battle_signal_bus.scripts.Systems;
using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Composition root for the investigation layer. Add as an autoload or scene node.
    /// Owns the Logbook/parser/runner and wires UI, portals, and confrontation to combat.
    /// </summary>
    public partial class InvestigationBootstrapper : Node
    {
        [Export] public NodePath CombatManagerPath;
        [Export] public NodePath QuestioningPanelPath;

        // Author all clue resources for the slice here; keyed by ClueId at _Ready.
        [Export] public Godot.Collections.Array<ClueEntry> AuthoredClues = new();

        public Logbook Logbook { get; private set; }
        public TopicParser Parser { get; private set; }
        public DialogueRunner Runner { get; private set; }
        public DeductionEngine Deduction { get; private set; }
        public ConfrontationResolver Confrontation { get; private set; }
        public PortalTravel Portals { get; private set; }

        private CombatManager _combat;
        private readonly Dictionary<string, ClueEntry> _cluesById = new();
        private readonly List<string> _playedEvents = new();
        private readonly List<WitnessState> _witnesses = new();

        public override void _Ready()
        {
            Logbook = new Logbook();
            Parser = new TopicParser();
            Runner = new DialogueRunner(Logbook, Parser);
            Deduction = new DeductionEngine(Logbook);
            Portals = new PortalTravel(Logbook);

            foreach (var clue in AuthoredClues)
                if (clue != null && !string.IsNullOrEmpty(clue.ClueId))
                    _cluesById[clue.ClueId] = clue;

            _combat = GetNodeOrNull<CombatManager>(CombatManagerPath);
            if (_combat != null)
                Confrontation = new ConfrontationResolver(Logbook, _combat);

            var panel = GetNodeOrNull<QuestioningPanel>(QuestioningPanelPath);
            panel?.Init(Parser);

            SignalBus.Instance.ClueMarked += _ =>
            {
                foreach (var fact in Deduction.Evaluate())
                    GD.Print($"[Deduction] Fact proven: {fact}");
            };

            SeedSliceContent();
            TryRestore();
        }

        private void SeedSliceContent()
        {
            Deduction.AddConvergence("outbound_logs", "warning_letter", "signal_was_answered");

            _witnesses.Add(new WitnessState
            {
                WitnessId = "scientist",
                Disposition = -20,
                Greed = 60,
                KnownTopics = new List<string> { "array", "logs", "signal" }
            });

            foreach (var w in _witnesses)
                Parser.Learn(w.KnownTopics);
        }

        private void TryRestore()
        {
            var data = InvestigationSave.Load();
            if (data == null) return;

            var location = InvestigationRestore.Apply(
                data, Logbook, Parser, _cluesById, _witnesses, _playedEvents);
            GD.Print($"[Investigation] Restored at {location}");
        }

        public async System.Threading.Tasks.Task PlayEcho(BattleEcho echo)
        {
            if (_combat == null) return;
            var (attacker, defender) = EchoContextFactory.Build(echo);
            await EchoRunner.Play(_combat, attacker, defender, echo);
            if (!_playedEvents.Contains(echo.EventId))
                _playedEvents.Add(echo.EventId);
        }

        public void SaveGame(Vector2 location) =>
            InvestigationSave.Save(Logbook, _witnesses, _playedEvents, location);
    }
}