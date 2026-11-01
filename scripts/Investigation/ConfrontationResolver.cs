using dot_battle_signal_bus.scripts.Systems;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Resolves an uncooperative witness. Threaten requires a MARKED incriminating clue
    /// as leverage; without it the witness resists and may escalate to HOMM combat.
    /// </summary>
    public class ConfrontationResolver
    {
        private readonly Logbook _logbook;
        private readonly CombatManager _combat;

        public ConfrontationResolver(Logbook logbook, CombatManager combat)
        {
            _logbook = logbook;
            _combat = combat;
        }

        public async Task<Approach> Resolve(
            WitnessState witness,
            string approach,
            List<BaseUnit> inquisitorParty,
            string leverageClueId = null)
        {
            switch (approach)
            {
                case "bribe":
                    return witness.Greed >= 50 ? Approach.Bribe : Approach.Talk;

                case "threaten":
                    if (leverageClueId != null && _logbook.HasMarked(leverageClueId))
                        return Approach.Threaten;

                    // No leverage => resistance => escalate to combat.
                    await StartFight(witness, inquisitorParty);
                    return Approach.Fight;

                case "fight":
                    await StartFight(witness, inquisitorParty);
                    return Approach.Fight;

                default:
                    return Approach.Talk;
            }
        }

        private async Task StartFight(WitnessState witness, List<BaseUnit> inquisitorParty)
        {
            SignalBus.Instance.EmitSignal(
                SignalBus.SignalName.ConfrontationResolved, witness.WitnessId, (int)Approach.Fight);

            // Reuse the existing combat engine untouched.
            await _combat.StartCombat(inquisitorParty, witness.Guards);
        }
    }
}
