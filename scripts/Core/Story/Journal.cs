using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Story
{
    public class Journal
    {
        public List<string> EvidenceCollected;
        public List<string> WitnessesInterviewed;
        public Dictionary<string, bool> Flags;
        public List<string> Notes;
        public Journal()
        {
            EvidenceCollected= new List<string>();
            WitnessesInterviewed = new List<string>();
            Notes = new List<string>();
            Flags = new Dictionary<string, bool>();
        }

        public void SetFlag(string flagName, bool value)
        {
            if (Flags.ContainsKey(flagName))
            {
                Flags[flagName] = value;
            }
            else
            {
                Flags.Add(flagName, value);
            }
        }

        public void AddEvidence(string evidenceId)
        {
            if (!EvidenceCollected.Contains(evidenceId))
            {
                EvidenceCollected.Add(evidenceId);
            }
        }


        public void AddWitness(string witnessId)
        {
            if (!WitnessesInterviewed.Contains(witnessId))
            {
                WitnessesInterviewed.Add(witnessId);
            }
        }
        public void AddNote(string note)
        {
                Notes.Add(note);
        }

    }
}
