using Godot;
using System.Collections.Generic;

namespace dot_battle_signal_bus.scripts.Combat
{
    public partial class CombatResult : Resource
    {
        public List<BaseUnit> Winners = new ();
        public List<BaseUnit> Losers = new List<BaseUnit>();
        public int TotalXPPool;

        // Track "Events" that happened during combat
        public Dictionary<BaseUnit, CombatStats> UnitStats = new();


        //TODO - Add more stats as needed, such as damage dealt, damage taken, etc.
        //add which unit was injured for ex. quest "critically hit 3 different units" or "land 5 critical hits on the same unit"
        public void AddCriticalHit(BaseUnit attacker)
        {
            if (!UnitStats.ContainsKey(attacker))
            {
                UnitStats[attacker] = new CombatStats();
            }
            UnitStats[attacker].CritsLanded += 1;            
        }

        public void AddLoss(BaseUnit defender)
        {
            if (!Losers.Contains(defender))
            {
                BaseUnit deadUnit = defender.Duplicate() as BaseUnit;
                deadUnit.Portrait = defender.Portrait.Duplicate() as TextureRect;
                Losers.Add(deadUnit);

            }
        }

        public void AddKill(BaseUnit attacker)
        {
            if (!UnitStats.ContainsKey(attacker))
            {
                UnitStats[attacker] = new CombatStats();
            }
            UnitStats[attacker].Kills += 1;

        }

        public void UnitDied(BaseUnit unit)
        {
            if (UnitStats.ContainsKey(unit))
            {
                UnitStats.Remove(unit); // Remove unit from stats tracking if they died, or we could keep them with a "Died" flag
            }
            var loosers = Losers.Count;
        }

        public void AddXPEarned(BaseUnit unit, int xp)
        {
            if (!UnitStats.ContainsKey(unit))
            {
                UnitStats[unit] = new CombatStats();
            }
            UnitStats[unit].XpEarned += xp;
        }
    }
}
