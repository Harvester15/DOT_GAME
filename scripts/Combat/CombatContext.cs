using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat
{
    public class CombatContext
    {
        public List<BaseUnit> _attacker;
        public List<BaseUnit> _defender;
        //public List<BaseUnit> Attacker;
        //public List<BaseUnit> Defender;
        
        public TerrainType Terrain;
        public CombatSettings Settings;

        public CombatContext(List<BaseUnit> attacker, List<BaseUnit> defender)
        {
            _attacker = attacker;
            _defender = defender;
        }
    }

}
