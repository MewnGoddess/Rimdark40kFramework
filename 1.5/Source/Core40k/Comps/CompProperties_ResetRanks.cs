using RimWorld;
using Verse;

namespace Core40k
{
    public class CompProperties_ResetRanks : CompProperties_AbilityGiveHediff
    {
        public int canDemoteToTierInclusive = 0;

        public RankCategoryDef rankCategoryDef = null;

        public CompProperties_ResetRanks()
        {
            compClass = typeof(CompAbilityEffect_ResetRanks);
        }
    }
}