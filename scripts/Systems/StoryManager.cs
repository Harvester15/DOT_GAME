using dot_battle_signal_bus.scripts.Core.Save;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Systems
{
        /// <summary>
        /// Owns all persistent narrative state (flags, variables, evidence, witnesses, notes)
        /// and the story-progression logic that reacts to it. Scene-independent autoload.
        /// </summary>
        public partial class StoryManager : Node
        {
            private static StoryManager _instance;
            public static StoryManager Instance => _instance;

            [Signal] public delegate void EvidenceAddedEventHandler(string evidenceId);
            [Signal] public delegate void WitnessUnlockedEventHandler(string witnessId);
            [Signal] public delegate void FlagChangedEventHandler(string flag, bool value);

            private readonly StoryState _state = new();

            public override void _Ready()
            {
                if (_instance == null) _instance = this;
                else QueueFree();
            }

            // ---------------- Flags & variables ----------------

            public void SetFlag(string flag, bool value = true)
            {
                _state.Flags[flag] = value;
                EmitSignal(SignalName.FlagChanged, flag, value);
                CheckEvents();
            }

            public bool GetFlag(string flag) =>
                _state.Flags.TryGetValue(flag, out var v) && (bool)v;

            public void SetVariable(string key, Variant value) => _state.Variables[key] = value;

            public Variant GetVariable(string key) =>
                _state.Variables.TryGetValue(key, out var v) ? v : default;

            // ---------------- Evidence & witnesses ----------------

            public void AddEvidence(string evidenceId)
            {
                if (_state.Evidence.Contains(evidenceId))
                    return;

                _state.Evidence.Add(evidenceId);
                EmitSignal(SignalName.EvidenceAdded, evidenceId);
                GD.Print($"Evidence collected: {evidenceId}");

                // New evidence may satisfy an unlock condition.
                CheckEvents();
            }

            public bool HasEvidence(string evidenceId) => _state.Evidence.Contains(evidenceId);

            public void AddWitness(string witnessId)
            {
                if (_state.Witnesses.Contains(witnessId))
                    return;

                _state.Witnesses.Add(witnessId);
                EmitSignal(SignalName.WitnessUnlocked, witnessId);
                GD.Print($"Witness unlocked: {witnessId}");
            }

            public void AddNote(string note) => _state.Notes.Add(note);

            // ---------------- Progression ----------------

            /// <summary>
            /// Central place that evaluates whether collected evidence/flags unlock
            /// new story beats. Called whenever relevant state changes.
            /// </summary>
            public void CheckEvents()
            {
                // Example rule: both clues present -> unlock the butler as a witness.
                if (HasEvidence("bloody_knife") && HasEvidence("torn_letter"))
                    AddWitness("the_butler");

                // Add further data-driven rules here (or load them from a resource).
            }

            // ---------------- Persistence hooks (used by SaveManager) ----------------

            /// <summary>Exposes the raw state for serialization. SaveManager owns disk I/O, not this.</summary>
            public StoryState CaptureState() => _state;

            public void RestoreState(StoryState loaded)
            {
                _state.Flags = loaded.Flags;
                _state.Variables = loaded.Variables;
                _state.Evidence = loaded.Evidence;
                _state.Witnesses = loaded.Witnesses;
                _state.Notes = loaded.Notes;
            }
        }
    }
