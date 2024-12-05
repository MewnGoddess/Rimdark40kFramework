using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Core40k
{
    public class Ability_MapWideHediff : VFECore.Abilities.Ability
    {
        private void AffectThings()
        {
            var pawnsToAffect = new List<Pawn>();

            var defMod = def.GetModExtension<DefModExtension_MapWideHediff>();
            
            if (defMod.affectEnemies)
            {
                pawnsToAffect.AddRange(CasterPawn.Map.mapPawns.AllPawnsSpawned.Where(p => p.Faction != Faction.OfPlayer));
            }
            
            if (defMod.affectPlayerColonists)
            {
                pawnsToAffect.AddRange(CasterPawn.Map.mapPawns.FreeColonistsSpawned);
            }
            
            if (!defMod.affectCaster && pawnsToAffect.Contains(CasterPawn))
            {
                pawnsToAffect.Remove(CasterPawn);
            }

            foreach (var affectedPawn in pawnsToAffect)
            {
                var hediffForPawn = HediffMaker.MakeHediff(defMod.hediffDef, affectedPawn);
                
                var hediffComp_Disappears = hediffForPawn.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = def.durationTime;
                }
                
                affectedPawn.health.AddHediff(hediffForPawn);
            }
        }
        
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            AffectThings();
            base.Cast(targets);
        }
    }
}