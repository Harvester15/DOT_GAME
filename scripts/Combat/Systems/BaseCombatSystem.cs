using dot_battle_signal_bus.scripts.Combat.Interface;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat.Systems
{
    public abstract partial class BaseCombatSystem : Node, ICombatSystem
    {
        protected CombatResult _combatResult = new();
        protected List<BaseUnit> _allUnits;

        // Shared logic for all systems
        protected void UpdateBattleReport(BaseUnit killer, BaseUnit victim)
        {
            _combatResult.AddKill(killer);
            _combatResult.TotalXPPool += victim.ExperienceReward;
            // ... (rest of your logic)
        }

        public abstract Task<CombatResult> Execute(CombatContext context);
    }
}
