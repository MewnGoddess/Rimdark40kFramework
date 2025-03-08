using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Core40k
{
    public class RankCategoryDef : Def
    {
        public GeneDef unlockedByGene;
        public HediffDef unlockedByHediff;
        public TraitDef unlockedByTrait;
        public int traitDegree = 0;

        public bool RankCategoryUnlockedFor(Pawn pawn)
        {
            if (unlockedByGene != null)
            {
                if (pawn.genes == null || !pawn.genes.HasActiveGene(unlockedByGene))
                {
                    return false;
                }
            }

            if (unlockedByTrait != null)
            {
                if (pawn.story == null || !pawn.story.traits.HasTrait(unlockedByTrait, traitDegree))
                {
                    return false;
                }
            }

            if (unlockedByHediff != null)
            {
                if (pawn.health == null || !pawn.health.hediffSet.HasHediff(unlockedByHediff))
                {
                    return false;
                }
            }

            return true;
        }
    }
}