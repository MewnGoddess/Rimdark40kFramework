using System.Collections.Generic;
using System.Linq;
using VEF.Abilities;
using Verse;

namespace Core40k;

public class CompRankInfo : ThingComp
{
    public CompProperties_RankInfo Props => (CompProperties_RankInfo)props;
        
    private List<RankDef> unlockedRanks = new List<RankDef>();

    public List<RankDef> unlockedRanksAtDeath = new List<RankDef>();
        
    public List<RankDef> UnlockedRanks => unlockedRanks;

    private RankCategoryDef lastOpenedRankCategory = null;
        
    public RankCategoryDef LastOpenedRankCategory => lastOpenedRankCategory;
        
    private Dictionary<RankDef, int> daysAsRank = new Dictionary<RankDef, int>();
        
    public Dictionary<RankDef, int> DaysAsRank => daysAsRank;

    private GameComponent_RankInfo gameComponentRankInfo = null;

    public GameComponent_RankInfo GameComponentRankInfo => gameComponentRankInfo ??= Current.Game.GetComponent<GameComponent_RankInfo>();

    public void UnlockRank(RankDef rank)
    {
        if (unlockedRanks.Contains(rank))
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }
            
        if (rank.rankTier > HighestRank())
        {
            pawn.story.Title = rank.label;
        }
            
        unlockedRanks.Add(rank);
            
        if (!daysAsRank.ContainsKey(rank))
        {
            daysAsRank.Add(rank, 0);
        }
            
        if (rank.givesAbilities != null)
        {
            foreach (var ability in rank.givesAbilities)
            {
                pawn.abilities.GainAbility(ability);
            }
        }
            
        if (rank.givesVFEAbilities != null)
        {
            var comp = pawn.GetComp<CompAbilities>();
            if (comp != null)
            {
                foreach (var ability in rank.givesVFEAbilities)
                {
                    comp.GiveAbility(ability);
                }
            }
        }
            
        var gameCompRankInfo = Current.Game.GetComponent<GameComponent_RankInfo>();

        if (gameCompRankInfo.rankLimits.ContainsKey(rank))
        {
            gameCompRankInfo.rankLimits[rank] += 1;
        }
        else
        {
            gameCompRankInfo.rankLimits.Add(rank, 1);
        }
    }

    public void RemoveRank(RankDef rankDef, bool removeFromRankLimit)
    {
        unlockedRanks.Remove(rankDef);
        daysAsRank.Remove(rankDef);
            
        if (parent is not Pawn pawn)
        {
            return;
        }
            
        if (rankDef.givesAbilities != null)
        {
            foreach (var ability in rankDef.givesAbilities)
            {
                pawn.abilities.RemoveAbility(ability);
            }
        }
            
        if (rankDef.givesVFEAbilities != null)
        {
            var comp = pawn.GetComp<CompAbilities>();
            if (comp != null)
            {
                foreach (var ability in rankDef.givesVFEAbilities)
                {
                    comp.LearnedAbilities.RemoveWhere(learnedAbility => learnedAbility.def == ability);
                }
            }
        }

        if (removeFromRankLimit)
        {
            GameComponentRankInfo.PawnLostRank(rankDef);
        }

        var newHighestRank = HighestRankDef(false);
        if (newHighestRank != null)
        {
            pawn.story.Title = newHighestRank.label;
        }
    }

    public int HighestRank()
    {
        if (unlockedRanks.NullOrEmpty())
        {
            return -1;
        }
            
        return unlockedRanks.MaxBy(rank => rank.rankTier).rankTier;
    }
        
    public int HighestRank(RankCategoryDef rankCategoryDef)
    {
        if (unlockedRanks.NullOrEmpty())
        {
            return -1;
        }

        var unlockedRanksOfDef = UnlockedRanksOfDef(rankCategoryDef).Where(HasRank).Select(rankDef => rankDef).ToList();
        if (unlockedRanksOfDef.NullOrEmpty())
        {
            return -1;
        }
            
        return unlockedRanksOfDef.MaxBy(rank => rank.rankTier).rankTier;
    }
        
    public RankDef HighestRankDef(bool onlySpecialist, RankCategoryDef rankCategoryDef)
    {
        
        var list = UnlockedRanksOfDef(rankCategoryDef).Where(def => !onlySpecialist || def.specialistRank).ToList();
            
        return list.NullOrEmpty() ? null : list.MaxBy(rank => rank.rankTier);
    }

    public List<RankDef> UnlockedRanksOfDef(RankCategoryDef rankCategoryDef)
    {
        var unlockedRanksOfDef = rankCategoryDef.ranks.Where(data => HasRank(data.rankDef)).Select(data => data.rankDef).ToList();
        return unlockedRanksOfDef;
    }
        
    public RankDef HighestRankDef(bool onlySpecialist)
    {
        var list = unlockedRanks.Where(def => !onlySpecialist || def.specialistRank).ToList();
            
        return list.NullOrEmpty() ? null : list.MaxBy(rank => rank.rankTier);
    }

    public bool HasRankOfCategory(RankCategoryDef rankCategoryDef)
    {
        return UnlockedRanksOfDef(rankCategoryDef).Any();
    }

    public void ResetRanks(RankCategoryDef rankCategoryDef)
    {
        if (rankCategoryDef != null)
        {
            foreach (var rankDef in UnlockedRanksOfDef(rankCategoryDef))
            {
                RemoveRank(rankDef, true);
            }
        }
        else
        {
            foreach (var rankDef in unlockedRanks)
            {
                RemoveRank(rankDef, true);
            }
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
        GameComponentRankInfo.PawnResetRanks(UnlockedRanks);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        GameComponentRankInfo.PawnResetRanks(UnlockedRanks);
    }
        
    public override void CompTick()
    {
        base.CompTick();
        if (parent is not Pawn pawn)
        {
            return;
        }

        if (!pawn.IsHashIntervalTick(60000))
        {
            return;
        }

        IncreaseDaysForAllRank();
    }

    public void IncreaseDaysForAllRank()
    {
        var daysAsRankTemp = daysAsRank.ToList();
        foreach (var rank in daysAsRankTemp)
        {
            daysAsRank[rank.Key]++;
        }
    }
        
    public void IncreaseDaysAsRank(RankDef rankDef)
    {
        daysAsRank[rankDef]++;
    }
        
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref unlockedRanks, "unlockedRanks", LookMode.Def);
        Scribe_Collections.Look(ref unlockedRanksAtDeath, "unlockedRanksAtDeath", LookMode.Def);
        Scribe_Collections.Look(ref daysAsRank, "daysAsRank");
        Scribe_Defs.Look(ref lastOpenedRankCategory, "lastOpenedRankCategory");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }
            
        daysAsRank ??= new Dictionary<RankDef, int>();
    }
        
}