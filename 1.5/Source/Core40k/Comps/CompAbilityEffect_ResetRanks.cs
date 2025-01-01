using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class CompAbilityEffect_ResetRanks : CompAbilityEffect
    {
        private new CompProperties_ResetRanks Props => (CompProperties_ResetRanks)props;

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

            var canDemoteTier = Props.canDemoteToTierInclusive;
            if (Props.ownRankAsTier)
            {
                canDemoteTier = parent.pawn.GetComp<CompRankInfo>().HighestRank();
            }
            
            return target.Pawn.GetComp<CompRankInfo>().HighestRank() <= canDemoteTier;
        }
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (!target.Pawn.HasComp<CompRankInfo>())
            {
                return "BEWH.Framework.RankSystem.DoesNotHaveRank".Translate(target.Pawn);
            }
            
            if (target.Pawn.GetComp<CompRankInfo>().UnlockedRanks.NullOrEmpty())
            {
                return "BEWH.Framework.RankSystem.NoUnlockedRanks".Translate(target.Pawn);
            }

            if (!target.Pawn.GetComp<CompRankInfo>().HasRankOfCategory(Props.rankCategoryDef))
            {
                return "BEWH.Framework.RankSystem.NoUnlockedRanksOfCategory".Translate(target.Pawn, Props.rankCategoryDef);
            }
            
            var canDemoteTier = Props.canDemoteToTierInclusive;
            if (Props.ownRankAsTier)
            {
                canDemoteTier = parent.pawn.GetComp<CompRankInfo>().HighestRank();
            }

            if (target.Pawn.GetComp<CompRankInfo>().HighestRank() > canDemoteTier)
            {
                return "BEWH.Framework.RankSystem.RankTooHigh".Translate(target.Pawn);
            }
            
            return null;
        }
    }
}