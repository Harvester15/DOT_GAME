using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dot_battle_signal_bus.scripts;

namespace dot_battle_signal_bus.scripts.Investigation.Services
{
    public class ConversationService
    {

        private WitnessEncounter _currentWitness;

        public void BeginConversation(
            WitnessData data,
            WitnessState state)
        {
            _currentWitness = new WitnessEncounter
            {
                Data = data,
                State = state
            };
        }
        public void Initialize()
        {
            //responses depends on mood?
            //catch response from TopicAsked signal, then emit a response signal with the line to display
            SignalBus.Instance.TopicAsked += GetResponseForTopic;
        }

        public void GetResponseForTopic(string topic)
        {
            ClueEntry clue = _currentWitness.Data.KnownClues.FirstOrDefault(x => x.ClueId == topic);
            if (clue != null)
            {
                SignalBus.Instance.EmitSignal(SignalBus.SignalName.DialogueLineShown, $"{clue.DisplayText}");
            }
            else
            {
                SignalBus.Instance.EmitSignal(SignalBus.SignalName.DialogueLineShown, $"I don't know about {topic}.");
            }

        }
    }
}