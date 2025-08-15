using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Core40k;

public class RankCategoryDef : Def
{
    public GeneDef unlockedByGene;
    public HediffDef unlockedByHediff;
    public TraitDef unlockedByTrait;
    public GeneDef lockedByGene;
    public HediffDef lockedByHediff;
    public TraitDef lockedByTrait;
    public int unlockTraitDegree = 0;
    public int lockTraitDegree = 0;

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
            if (pawn.story == null || !pawn.story.traits.HasTrait(unlockedByTrait, unlockTraitDegree))
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
        
        if (lockedByGene != null)
        {
            if (pawn.genes != null && pawn.genes.HasActiveGene(lockedByGene))
            {
                return false;
            }
        }

        if (lockedByTrait != null)
        {
            if (pawn.story != null && pawn.story.traits.HasTrait(lockedByTrait, lockTraitDegree))
            {
                return false;
            }
        }

        if (lockedByHediff != null)
        {
            if (pawn.health != null && pawn.health.hediffSet.HasHediff(lockedByHediff))
            {
                return false;
            }
        }

        return true;
    }
    
    public string RankCategoryRequirementsNotMetFor(Pawn pawn)
    {
        var stringBuilder = new StringBuilder();
        var allRequirementUnlock = new List<string>();
        var allRequirementLock = new List<string>();
            
        if (pawn.genes != null && unlockedByGene != null && !pawn.genes.HasActiveGene(unlockedByGene))
        {
            allRequirementUnlock.Add("BEWH.Framework.RankSystem.CategoryRequiredGene".Translate(unlockedByGene.label.CapitalizeFirst()));
        }
            
        if (unlockedByHediff != null && !pawn.health.hediffSet.HasHediff(unlockedByHediff))
        {
            allRequirementUnlock.Add("BEWH.Framework.RankSystem.CategoryRequiredHediff".Translate(unlockedByHediff.label.CapitalizeFirst()));
        }
            
        if (unlockedByTrait != null && !pawn.story.traits.HasTrait(unlockedByTrait, unlockTraitDegree))
        {
            allRequirementUnlock.Add("BEWH.Framework.RankSystem.CategoryRequiredTrait".Translate(unlockedByTrait.DataAtDegree(unlockTraitDegree).label.CapitalizeFirst()));
        }

        if (allRequirementUnlock.Count > 0)
        {
            stringBuilder.Append("BEWH.Framework.RankSystem.CategoryRequiresUnlock".Translate(label.CapitalizeFirst()));
            for (var i = 0; i < allRequirementUnlock.Count; i++)
            {
                stringBuilder.Append(allRequirementUnlock[i]);
                if (i + 2 == allRequirementUnlock.Count)
                {
                    stringBuilder.Append("BEWH.Framework.RankSystem.And".Translate());
                }
                else if (i + 2 < allRequirementUnlock.Count)
                {
                    stringBuilder.Append(", ");
                }
            }
        }
        
        if (pawn.genes != null && lockedByGene != null && pawn.genes.HasActiveGene(lockedByGene))
        {
            allRequirementLock.Add("BEWH.Framework.RankSystem.CategoryRequiredGene".Translate(lockedByGene.label.CapitalizeFirst()));
        }
            
        if (lockedByHediff != null && pawn.health.hediffSet.HasHediff(lockedByHediff))
        {
            allRequirementLock.Add("BEWH.Framework.RankSystem.CategoryRequiredHediff".Translate(lockedByHediff.label.CapitalizeFirst()));
        }
            
        if (lockedByTrait != null && pawn.story.traits.HasTrait(lockedByTrait, lockTraitDegree))
        {
            allRequirementLock.Add("BEWH.Framework.RankSystem.CategoryRequiredTrait".Translate(lockedByTrait.DataAtDegree(lockTraitDegree).label.CapitalizeFirst()));
        }
        //locked by stuff here and then test
        
        if (allRequirementLock.Count > 0)
        {
            stringBuilder.AppendLineIfNotEmpty();
            stringBuilder.Append("BEWH.Framework.RankSystem.CategoryRequiresLock".Translate(label.CapitalizeFirst()));
            for (var i = 0; i < allRequirementLock.Count; i++)
            {
                stringBuilder.Append(allRequirementLock[i]);
                if (i + 2 == allRequirementLock.Count)
                {
                    stringBuilder.Append("BEWH.Framework.RankSystem.And".Translate());
                }
                else if (i + 2 < allRequirementLock.Count)
                {
                    stringBuilder.Append(", ");
                }
            }
        }   

        return stringBuilder.ToString();
    }
}