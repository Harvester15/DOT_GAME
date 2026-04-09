using dot_battle_signal_bus.scripts.Combat;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Systems
{
    public partial class ProgressionManager : Node2D
    {
        public override void _Ready()
        {
            SignalBus.Instance.CombatResolved += OnCombatResolved;
        }

        private void OnCombatResolved(CombatResult result)
        {
            // 1. Handle XP & Leveling
            DistributeXP(result.Winners, result.TotalXPPool);

            // 2. Handle Skill/Ability Improvement (based on crits, etc.)
            foreach (var unitEntry in result.UnitStats)
            {
                ProcessSkillProgression(unitEntry.Key, unitEntry.Value);
            }

            // 3. Handle Fame/Reputation
            UpdateFame(result);

            // 4. Update Quests (e.g., "Kill 10 Orcs")
            //TODO implement quest system and update progress here
            //QuestManager.Instance.UpdateProgress(result);
        }

        //TODO not only for killing units
        private void DistributeXP(List<BaseUnit> winners, int totalXPPool)
        {
            throw new NotImplementedException();
        }

        private void UpdateFame(CombatResult result)
        {
            throw new NotImplementedException();
        }

        private void ProcessSkillProgression(BaseUnit unit, CombatStats stats)
        {
            if (stats.CritsLanded > 0)
            {
                // AD&D Style: Practicing a skill makes you better at it
                //TODO this is a very simple example, we could have more complex logic based on weapon types, skill categories, etc.
                //unit.CritChanceProgression += stats.CritsLanded * 0.1f;
            }
        }
    }

}
