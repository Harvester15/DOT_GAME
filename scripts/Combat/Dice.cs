using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat
{

    public static class Dice
    {
        private static RandomNumberGenerator rng = new RandomNumberGenerator();

        // Call this once at game start to seed
        public static void Init()
        {
            rng.Randomize();
        }

        public static int RollD20()
        {
            return rng.RandiRange(1, 20);
        }

        public static int RollD10()
        {
            return rng.RandiRange(1, 10);
        }

        public static int Roll(int count, int sides, int modifier = 0)
        {
            int total = modifier;
            for (int i = 0; i < count; i++)
            {
                total += rng.RandiRange(1, sides);
            }
            return total;
        }

        // Convenience for damage dice from a Unit
        public static int RollDamage(BaseUnit unit)
        {
            if (unit == null)
            {
                GD.PrintErr("RollDamage: unit is null.");
                return 0;
            }

            // Prefer the UnitData damage, fall back to the unit's own Dmg if the resource is incomplete.
            Damage dmg = unit.Data?.Dmg ?? unit.Dmg;

            if (dmg == null)
            {
                GD.PrintErr($"RollDamage: no Damage data for '{unit.Name}' " +
                            $"(Data={(unit.Data == null ? "null" : "set")}, Dmg={(unit.Dmg == null ? "null" : "set")}).");
                return 0;
            }

            return Roll(unit.Data.Dmg.DamageCount, unit.Data.Dmg.DamageSides, unit.Data.Dmg.DamageModifier);
        }
    }
}
