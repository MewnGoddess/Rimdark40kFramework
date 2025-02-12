using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Core40k
{
    public class DamageWorker_Holy : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            var damageResult = new DamageResult
            {
                totalDamageDealt = 0
            };
            
            if (!(victim is Pawn victimPawn))
            {
                return damageResult;
            }

            var defMod = def.GetModExtension<DefModExtension_HolyDamageExtension>();

            if (victimPawn.HostileTo(Faction.OfPlayer))
            {
                var rnd = new Random();
                var hitAmount = rnd.Next(defMod.minHitAmount, defMod.maxHitAmount);
                
                var damageAmount = dinfo.Amount;
                
                for (var i = 0; i < hitAmount; i++)
                {
                    if (victimPawn.Dead || victimPawn.Destroyed)
                    {
                        continue;
                    }
                    
                    damageResult = base.Apply(new DamageInfo(dinfo.Def, damageAmount, 999f, instigator: dinfo.Instigator), victimPawn);
                }

                if (rnd.Next(0, 100) < defMod.chanceToIgnite)
                {
                    victimPawn.TryAttachFire(1, dinfo.Instigator);
                }
            }
            else
            {
                var healAmount = dinfo.Amount * defMod.healPercentOfDamageToAllies;

                var injuries = victimPawn.health.hediffSet.hediffs.Where(hediff => hediff is Hediff_Injury).Cast<Hediff_Injury>().ToList();

                healAmount /= injuries.Count;

                foreach (var injury in injuries)
                {
                    injury.Heal(healAmount);
                }
            }
            
            return damageResult;
        }
    }

}