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
        
        private Dictionary<RankDef, int> daysAsRank = new Dictionary<RankDef, int>();
        
        public Dictionary<RankDef, int> DaysAsRank => daysAsRank;

        public void UnlockRank(RankDef rank)
        {
            if (unlockedRanks.Contains(rank))
            {
                return;
            }
            
            unlockedRanks.Add(rank);
            
            if (!daysAsRank.ContainsKey(rank))
            {
                daysAsRank.Add(rank, 0);
            }
        }

        public int HighestRank()
        {
            return unlockedRanks.MaxBy(rank => rank.rankTier).rankTier;
        }

        public bool HasRankOfCategory(RankCategoryDef rankCategoryDef)
        {
            return unlockedRanks.Any(rank => rank.rankCategory == rankCategoryDef);
        }

        public void ResetRanks(RankCategoryDef rankCategoryDef)
        {
            DecreaseRankLimitCountIfNecessary();
            if (rankCategoryDef != null)
            {
                foreach (var rankDef in unlockedRanks.ToList().Where(rankDef => rankDef.rankCategory == rankCategoryDef))
                {
                    unlockedRanks.Remove(rankDef);
                    daysAsRank.Remove(rankDef);
                }
            }
            else
            {
                unlockedRanks.Clear();
                daysAsRank.Clear();  
            }
        }

        public bool HasRank(RankDef rankDef)
        {
            return unlockedRanks.Contains(rankDef);
        }
        
        public void OpenedRankCategory(RankCategoryDef rankCategory)
        {
            lastOpenedRankCategory = rankCategory;
        }
        
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            base.Notify_Killed(prevMap, dinfo);
            DecreaseRankLimitCountIfNecessary();
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DecreaseRankLimitCountIfNecessary();
        }
        
        public override void CompTick()
        {
            base.CompTick();
            if (!(parent is Pawn pawn))
            {
                return;
            }

            if (!pawn.IsHashIntervalTick(60000))
            {
                return;
            }

            IncreaseDaysAsRank();
        }

        public void IncreaseDaysAsRank()
        {
            var daysAsRankTemp = daysAsRank.ToList();
            foreach (var rank in daysAsRankTemp)
            {
                daysAsRank[rank.Key]++;
            }
        }

        private void DecreaseRankLimitCountIfNecessary()
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
            Scribe_Collections.Look(ref daysAsRank, "daysAsRank");
            Scribe_Defs.Look(ref lastOpenedRankCategory, "lastOpenedRankCategory");

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }
            
            if (daysAsRank == null)
            {
                daysAsRank = new Dictionary<RankDef, int>();
            }
        }
    }
}