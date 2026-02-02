using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VEF.Abilities;
using Verse;

namespace Core40k;

public class CompRankInfo : ThingComp
{
    private const int TicksPerDay = 60000;
    public CompProperties_RankInfo Props => (CompProperties_RankInfo)props;
        
    private List<RankDef> unlockedRanks = new List<RankDef>();

    public List<RankDef> unlockedRanksAtDeath = new List<RankDef>();

    public List<RankDef> UnlockedRanks
    {
        get
        {
            unlockedRanks ??= [];
            if (unlockedRanks.Contains(null))
            {
                unlockedRanks.RemoveAll(def => def == null);
            }
            
            return unlockedRanks;
        }
    }

    private RankCategoryDef lastOpenedRankCategory = null;
        
    public RankCategoryDef LastOpenedRankCategory => lastOpenedRankCategory;

    private bool convertToStartTick = true;
    
    private Dictionary<RankDef, int> daysAsRank = new Dictionary<RankDef, int>();
    
    public Dictionary<SkillDef, Passion> originalPassions = new Dictionary<SkillDef, Passion>();

    private GameComponent_RankInfo gameComponentRankInfo = null;

    public GameComponent_RankInfo GameComponentRankInfo => gameComponentRankInfo ??= Current.Game.GetComponent<GameComponent_RankInfo>();
    
    public List<SkillDef> recreationSkillFromRanks
    {
        get
        {
            List<SkillDef> recreationSkillFromRanks = [];
            foreach (var rank in UnlockedRanks)
            {
                recreationSkillFromRanks.AddRange(rank.recreationFromSkills);
            }

            return recreationSkillFromRanks;
        }
    }

    public Pawn ParentPawn => parent as Pawn;
    
    public void UnlockRank(RankDef rank)
    {
        if (UnlockedRanks.Contains(rank))
        {
            return;
        }

        if (ParentPawn == null)
        {
            return;
        }
            
        if (rank.rankTier > HighestRank())
        {
            ParentPawn.story.Title = rank.label;
        }
            
        UnlockedRanks.Add(rank);
            
        if (!daysAsRank.ContainsKey(rank))
        {
            daysAsRank.Add(rank, Find.TickManager.TicksGame);
        }
        
        rank.UnlockRank(this);
            
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

    public void RemoveRank(RankDef rank, bool removeFromRankLimit)
    {
        UnlockedRanks.Remove(rank);
        daysAsRank.Remove(rank);
            
        if (parent is not Pawn pawn)
        {
            return;
        }
            
        rank.RemoveRank(this);

        if (removeFromRankLimit)
        {
            GameComponentRankInfo.PawnLostRank(rank);
        }

        var newHighestRank = HighestRankDef(false);
        if (newHighestRank != null)
        {
            pawn.story.Title = newHighestRank.label;
        }
    }
    
    public void RecalculatePassions()
    {
        foreach (var originalPassion in originalPassions)
        {
            var skill = ParentPawn.skills.GetSkill(originalPassion.Key);
            skill.passion = originalPassion.Value;
        }
        var passionMods = unlockedRanks.SelectMany(def => def.givesPassions);
        var skillDefPassionCol = new Dictionary<SkillDef, List<PassionMod.PassionModType>>();
        foreach (var passionMod in passionMods)
        {
            if (!skillDefPassionCol.ContainsKey(passionMod.skill))
            {
                skillDefPassionCol.Add(passionMod.skill, [passionMod.modType]);
            }
            else
            {
                skillDefPassionCol[passionMod.skill].Add(passionMod.modType);
            }
        }

        foreach (var col in skillDefPassionCol)
        {
            var skill = ParentPawn.skills.GetSkill(col.Key);
            if (col.Value.NullOrEmpty())
            {
                skill.passion = originalPassions[col.Key];
                originalPassions.Remove(col.Key);
                continue;
            }
            
            if (col.Value.Any(type => type == PassionMod.PassionModType.DropAll))
            {
                skill.passion = Passion.None;
                continue;
            }

            foreach (var passion in col.Value)
            {
                if (passion == PassionMod.PassionModType.AddOneLevel)
                {
                    skill.passion = skill.passion switch
                    {
                        Passion.None => Passion.Minor,
                        Passion.Minor => Passion.Major,
                        _ => skill.passion
                    };
                }
            }
        }
    }

    public int HighestRank()
    {
        if (UnlockedRanks.NullOrEmpty())
        {
            return -1;
        }
            
        return UnlockedRanks.MaxBy(rank => rank.rankTier).rankTier;
    }

    public float GetDaysAsRank(RankDef rankDef)
    {
        if (daysAsRank.TryGetValue(rankDef, out var days))
        {
            return Math.Abs((float)(days - Find.TickManager.TicksGame)) / TicksPerDay;
        }
        
        return 0f;
    }
    
    public int HighestRank(RankCategoryDef rankCategoryDef)
    {
        if (UnlockedRanks.NullOrEmpty())
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
    
    public RankDef HighestRankDef(bool onlySpecialist)
    {
        var list = UnlockedRanks.Where(def => !onlySpecialist || def.specialistRank).ToList();
            
        return list.NullOrEmpty() ? null : list.MaxBy(rank => rank.rankTier);
    }

    public List<RankDef> UnlockedRanksOfDef(RankCategoryDef rankCategoryDef)
    {
        var unlockedRanksOfDef = rankCategoryDef.ranks.Where(data => HasRank(data.rankDef)).Select(data => data.rankDef).ToList();
        return unlockedRanksOfDef;
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
            foreach (var rankDef in UnlockedRanks)
            {
                RemoveRank(rankDef, true);
            }
        }
    }

    public bool HasRank(RankDef rankDef)
    {
        return UnlockedRanks.Contains(rankDef);
    }
        
    public void OpenedRankCategory(RankCategoryDef rankCategory)
    {
        lastOpenedRankCategory = rankCategory;
    }
        
    public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
    {
        base.Notify_Killed(prevMap, dinfo);
        foreach (var rank in UnlockedRanks)
        {
            rank.Notify_Killed(this, prevMap, dinfo);
        }
        GameComponentRankInfo.PawnResetRanks(UnlockedRanks);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        GameComponentRankInfo.PawnResetRanks(UnlockedRanks);
    }

    public void IncreaseDaysForAllRank()
    {
        var daysAsRankTemp = daysAsRank.ToList();
        foreach (var rank in daysAsRankTemp)
        {
            IncreaseDaysAsRank(rank.Key);
        }
    }
        
    public void IncreaseDaysAsRank(RankDef rankDef)
    {
        daysAsRank[rankDef] -= TicksPerDay;
    }
        
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref unlockedRanks, "unlockedRanks", LookMode.Def);
        Scribe_Collections.Look(ref unlockedRanksAtDeath, "unlockedRanksAtDeath", LookMode.Def);
        Scribe_Collections.Look(ref daysAsRank, "daysAsRank");
        Scribe_Values.Look(ref convertToStartTick, "convertToStartTick");
        Scribe_Defs.Look(ref lastOpenedRankCategory, "lastOpenedRankCategory");

        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }
            
        daysAsRank ??= new Dictionary<RankDef, int>();

        if (!convertToStartTick)
        {
            return;
        }
        
        foreach (var keyPair in daysAsRank)
        {
            daysAsRank[keyPair.Key] = Find.TickManager.TicksGame - keyPair.Value;
        }

        convertToStartTick = false;
    }
        
}