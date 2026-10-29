using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class StoryManager : Node
    {
        public Dictionary<string, bool> Flags = new();
        public Dictionary<string, int> Variables = new();
        public List<string> Evidence = new();
        public List<string> Witnesses = new();

        public void SetFlag(string flag, bool value)
        {
            Flags[flag] = value;
        }
        public bool GetFlag(string flag)
        {
            return Flags.ContainsKey(flag) && Flags[flag];
        }

        public void SetVariable(string variable, int value)
        {
            Variables[variable] = value;
        }
        public int GetVariable(string variable)
        {
            return Variables.ContainsKey(variable) ? Variables[variable] : 0;
        }

        public void AddEvidence(string evidence)
        {
            if (!Evidence.Contains(evidence))
            {
                Evidence.Add(evidence);
            }
        }
        public void AddWitness(string witness)
        {
            if (!Witnesses.Contains(witness))
            {
                Witnesses.Add(witness);
            }
        }
    }
}
