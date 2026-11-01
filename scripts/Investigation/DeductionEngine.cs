using System.Collections.Generic;
using System.Linq;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Two marked clues that agree lock a Fact. The final accusation is judged against
    /// locked facts, never a scripted flag — multiple evidence paths stay valid.
    /// </summary>
    public class DeductionEngine
    {
        private readonly Logbook _logbook;

        // Convergence: both clue ids marked => this fact is proven.
        private readonly List<(string clueA, string clueB, string factId)> _convergences = new();
        private readonly HashSet<string> _lockedFacts = new();

        public DeductionEngine(Logbook logbook) => _logbook = logbook;

        public void AddConvergence(string clueA, string clueB, string factId) =>
            _convergences.Add((clueA, clueB, factId));

        /// <summary>Re-evaluate after any Mark. Returns newly locked facts.</summary>
        public IEnumerable<string> Evaluate()
        {
            var newly = new List<string>();
            foreach (var (a, b, fact) in _convergences)
            {
                if (_lockedFacts.Contains(fact)) continue;
                if (_logbook.HasMarked(a) && _logbook.HasMarked(b))
                {
                    _lockedFacts.Add(fact);
                    newly.Add(fact);
                }
            }
            return newly;
        }

        public bool IsProven(string factId) => _lockedFacts.Contains(factId);

        /// <summary>Accusation succeeds if every required fact is locked.</summary>
        public bool EvaluateAccusation(IEnumerable<string> requiredFacts) =>
            requiredFacts.All(_lockedFacts.Contains);
    }
}
