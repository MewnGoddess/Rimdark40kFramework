using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (victimPawn.Faction == null || !victimPawn.Faction.IsPlayer)
            {
                var rnd = new Random();
                var hitAmount = rnd.Next(2, 5);
                
                var damageAmount = dinfo.Amount;
                
                for (var i = 0; i < hitAmount; i++)
                {
                    if (victimPawn.Dead || victimPawn.Destroyed)
                    {
                        continue;
                    }
                    
                    base.Apply(new DamageInfo(dinfo.Def, damageAmount, 999f, instigator: dinfo.Instigator), victimPawn);
                }
            }
            
            var healAmount = dinfo.Amount / 2;

            var injuries = victimPawn.health.hediffSet.hediffs.Where(hediff => hediff is Hediff_Injury).Cast<Hediff_Injury>().ToList();

            healAmount /= injuries.Count;

            foreach (var injury in injuries)
            {
                injury.Heal(healAmount);
            }
            
            return damageResult;
        }
    }

}