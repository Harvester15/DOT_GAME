using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public enum Approach { Talk, Bribe, Threaten, Fight }

    public class WitnessState
    {
        public string WitnessId;
        public int Disposition;              // -100..100; higher = cooperative
        public int Greed;                    // 0..100 bribe susceptibility
        public List<BaseUnit> Guards = new();      // becomes defender on Fight
        public List<string> KnownTopics = new();   // topics this witness can answer
        public List<string> WithheldClues = new(); // released only via the right approach
    }
}
