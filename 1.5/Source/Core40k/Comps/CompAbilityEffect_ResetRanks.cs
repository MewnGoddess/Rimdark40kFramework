using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class CompAbilityEffect_ResetRanks : CompAbilityEffect_GiveHediff
    {
        public CompProperties_ResetRanks Props => (CompProperties_ResetRanks)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            target.Pawn.GetComp<CompRankInfo>().ResetRanks(Props.rankCategoryDef);
        }
        
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            base.Valid(target, throwMessages);
            if (!target.Pawn.HasComp<CompRankInfo>())
            {
                return false;
            }

            if (target.Pawn.GetComp<CompRankInfo>().UnlockedRanks.NullOrEmpty())
            {
                return false;
            }
            
            if (!target.Pawn.GetComp<CompRankInfo>().HasRankOfCategory(Props.rankCategoryDef))
            {
                return false;
            }
            
            return target.Pawn.GetComp<CompRankInfo>().HighestRank() < Props.canDemoteToTierInclusive;
        }
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (!target.Pawn.HasComp<CompRankInfo>())
            {
                return "BEWH.DoesNotHaveRank".Translate(target.Pawn);
            }
            
            if (target.Pawn.GetComp<CompRankInfo>().UnlockedRanks.NullOrEmpty())
            {
                return "BEWH.NoUnlockedRanks".Translate(target.Pawn);
            }

            if (!target.Pawn.GetComp<CompRankInfo>().HasRankOfCategory(Props.rankCategoryDef))
            {
                return "BEWH.NoUnlockedRanksOfCategory".Translate(target.Pawn, Props.rankCategoryDef);
            }

            if (target.Pawn.GetComp<CompRankInfo>().HighestRank() > Props.canDemoteToTierInclusive)
            {
                return "BEWH.RankTooHigh".Translate(target.Pawn);
            }
            
            return null;
        }
    }
}