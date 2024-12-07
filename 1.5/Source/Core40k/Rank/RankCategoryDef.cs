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
    }
}