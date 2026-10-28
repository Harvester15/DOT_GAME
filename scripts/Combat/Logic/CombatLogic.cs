using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Combat.Logic;

public static class CombatRules
{
    public struct AttackResult
    {
        public bool IsHit;
        public bool IsCritical;
        public int Damage;
        public string LogMessage;
    }

    public static AttackResult CalculateAttack(BaseUnit attacker, BaseUnit defender)
    {
        int attackRoll = Dice.RollD20();
        int totalAttack = attackRoll + attacker.Attack; //add other modifiers/buffs/bonuses
        int defenseValue = defender.Defense; //add other modifiers/buffs/bonuses

        //get first shield in array
       
        var shieldEffect = CalculateShieldLogic(attacker, defender);
        //adding shields if present
        defenseValue = defenseValue * shieldEffect;

        bool isCritical = attackRoll >= 20;
        bool isFumble = attackRoll <= 1;

        bool hit = !isFumble && (isCritical || totalAttack >= defenseValue);
        int finalDamage = 0;
        string message = "";

        if (hit)
        {
            int multiplier = isCritical ? 2 : 1;
            finalDamage = Dice.RollDamage(attacker) * multiplier; //add other multipliers/buffs/bonuses
            message = $"{attacker.Name} hits {defender.Name} for {finalDamage} damage!";
            if (isCritical) message = "[CRITICAL] " + message;
        }
        else
        {
            message = $"{attacker.Name} misses {defender.Name}!";
        }

        return new AttackResult
        {
            IsHit = hit,
            IsCritical = isCritical,
            Damage = finalDamage,
            LogMessage = message
        };
    }

    private static int CalculateShieldLogic(BaseUnit attacker, BaseUnit defender)
    {
        int damageReduction = 1; // No reduction by default
        ShieldData shieldData = new ShieldData();
        if (defender.ShieldLayers == null)
            return 1; 

        if (defender.ShieldLayers.Count > 0)
        {
            
            shieldData = defender.ShieldLayers[0];
            if (shieldData.Weaknesses.Any(w => w == attacker.Data.Dmg.dmgType))
            {
                SignalBus.Instance.EmitSignal("ShieldHit", defender, attacker, shieldData);
            }
        }
        
        return damageReduction;
    }
}
