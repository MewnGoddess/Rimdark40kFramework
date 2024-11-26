using System.Collections.Generic;
using System.Linq;
using Verse;


namespace Core40k
{
    public class CompRankInfo : ThingComp
    {
        public CompProperties_RankInfo Props => (CompProperties_RankInfo)props;
        
        private List<RankDef> unlockedRanks = new List<RankDef>();

        public List<RankDef> UnlockedRanks => unlockedRanks;

        private RankCategoryDef lastOpenedRankCategory = null;
        public RankCategoryDef LastOpenedRankCategory => lastOpenedRankCategory;

        public void UnlockRank(RankDef rank)
        {
            if (!unlockedRanks.Contains(rank))
            {
                unlockedRanks.Add(rank);
            }
        }

        public void OpenedRankCategory(RankCategoryDef rankCategory)
        {
            lastOpenedRankCategory = rankCategory;
        }
        
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            DecreaseRankLimitCountIfnecessary();
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DecreaseRankLimitCountIfnecessary();
        }

        private void DecreaseRankLimitCountIfnecessary()
        {
            var gameComp = Current.Game.GetComponent<GameComponent_RankInfo>();
            foreach (var rank in UnlockedRanks.Where(rank => rank.colonyLimitOfRank.x > 0 || (rank.colonyLimitOfRank.x == 0 && rank.colonyLimitOfRank.y > 0)))
            {
                if (!gameComp.rankLimits.ContainsKey(rank))
                {
                    continue;
                }
                if (gameComp.rankLimits[rank] == 1)
                {
                    gameComp.rankLimits.Remove(rank);
                }
                else
                {
                    gameComp.rankLimits[rank] -= 1;
                }
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref unlockedRanks, "unlockedRanks", LookMode.Def);
            Scribe_Defs.Look(ref lastOpenedRankCategory, "lastOpenedRankCategory");
        }
    }
}