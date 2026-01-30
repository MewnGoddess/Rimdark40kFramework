using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class RankDef : Def
{
    [NoTranslate]
    public string rankIconPath;
        
    public List<RankDef> incompatibleRanks = new List<RankDef>();

    public List<Aptitude> requiredSkills = new List<Aptitude>();
        
    public List<GeneDef> requiredGenesAll = new List<GeneDef>();
        
    public List<GeneDef> requiredGenesOneAmong = new List<GeneDef>();
        
    public List<TraitData> requiredTraitsAll = new List<TraitData>();
        
    public List<TraitData> requiredTraitsOneAmong = new List<TraitData>();
        
    public List<StatModifier> statOffsets = new List<StatModifier>();

    public List<StatModifier> statFactors = new List<StatModifier>();

    public List<ConditionalStatAffecter> conditionalStatAffecters = new List<ConditionalStatAffecter>();
        
    public List<AbilityDef> givesAbilities = new List<AbilityDef>();
        
    public List<VEF.Abilities.AbilityDef> givesVFEAbilities = new List<VEF.Abilities.AbilityDef>();
    
    public List<HediffData> givesHediffs = new List<HediffData>();
    
    public List<SkillDef> recreationFromSkills = new List<SkillDef>();
    
    public List<PassionMod> givesPassions = new List<PassionMod>();
        
    public List<string> customEffectDescriptions = new List<string>();
        
    public Vector2 colonyLimitOfRank = new Vector2(-1, -1);

    public bool defaultFirstRank = false;

    public int rankTier = 0;
        
    public bool specialistRank = false;
        
    [Unsaved]
    private Texture2D rankIcon;

    public Texture2D RankIcon
    {
        get
        {
            if (rankIcon != null)
            {
                return rankIcon;
            }
                
            rankIcon = !rankIconPath.NullOrEmpty() ? ContentFinder<Texture2D>.Get(rankIconPath) : BaseContent.BadTex;
            return rankIcon;
        }
    }
        
}