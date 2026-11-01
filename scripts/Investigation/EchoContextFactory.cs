using dot_battle_signal_bus.scripts;
using dot_battle_signal_bus.scripts.Actors.Units;
using dot_battle_signal_bus.scripts.Combat;
using System.Collections.Generic;
using static BaseGameObject;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Turns a biased BattleEcho (authored UnitData rosters) into live BaseUnit lists.
    /// Bias (count inflation) is baked here so the combat engine stays witness-agnostic.
    /// </summary>
    public static class EchoContextFactory
    {
        public static (List<BaseUnit> attacker, List<BaseUnit> defender) Build(BattleEcho echo)
        {
            var settings = echo.Settings ?? new CombatSettings();
            settings.WitnessId = echo.WitnessId;
            settings.EventId = echo.EventId;

            var attacker = Spawn(echo.MySide, Team.Player);
            var defender = Spawn(echo.TheirSide, Team.Enemy);

            if (settings.InflateCounts && settings.InflateBy > 0 && echo.TheirSide.Count > 0)
            {
                // "There were so many of them" — duplicate the first defender type.
                var template = echo.TheirSide[0];
                for (int i = 0; i < settings.InflateBy; i++)
                    defender.Add(MakeUnit(template, Team.Enemy));
            }

            return (attacker, defender);
        }

        private static List<BaseUnit> Spawn(Godot.Collections.Array<UnitData> roster, Team team)
        {
            var list = new List<BaseUnit>();
            foreach (var data in roster)
                list.Add(MakeUnit(data, team));
            return list;
        }

        private static BaseUnit MakeUnit(UnitData data, Team team)
        {
            // Minimal spawn: assign authored data + team. Swap for your unit scene
            // instantiation if BaseUnit needs a PackedScene (portrait/collision setup).
            var unit = new BaseUnit
            {
                Data = data,
                UnitTeam = team,
                UnitName = data?.UnitName ?? "Unit"
            };
            return unit;
        }
    }
}