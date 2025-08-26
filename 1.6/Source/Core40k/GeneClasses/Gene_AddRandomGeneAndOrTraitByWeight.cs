using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k;

public class Gene_AddRandomGeneAndOrTraitByWeight : Gene
{
    private static GeneDef chosenGene = null;
    private static List<GeneDef> chosenGenes = new List<GeneDef>();

    private static TraitDef chosenTrait = null;
    private static int chosenTraitDegree = 0;
    private static Dictionary<TraitDef, int> chosenTraits = new Dictionary<TraitDef, int>();

    public override void PostMake()
    {
        SelectGeneToGive();
        SelectTraitToGive();
    }
    public override void PostAdd()
    {
        base.PostAdd();
        AddSelectedTraitAndGene();
    }
    public override void PostRemove()
    {
        base.PostRemove();
        RemoveSelectedTraitAndGene();
    }
            
    private void AddSelectedTraitAndGene()
    {
        if (chosenGene != null)
        {
            pawn.genes.AddGene(chosenGene, true);
        }
        if (!chosenGenes.NullOrEmpty())
        {
            foreach (var gene in chosenGenes)
            {
                pawn.genes.AddGene(gene, true);
            }
        }

        if (chosenTrait != null)
        {
            var trait = new Trait(chosenTrait, chosenTraitDegree);
            pawn.story.traits.GainTrait(trait);
        }
        if (!chosenTraits.NullOrEmpty())
        {
            foreach (var traitPair in chosenTraits)
            {
                var trait = new Trait(traitPair.Key, traitPair.Value);
                pawn.story.traits.GainTrait(trait);
            }
        }
    }
        
    private void RemoveSelectedTraitAndGene()
    {
        if (chosenGene != null)
        {
            var gene = pawn.genes.GetGene(chosenGene);
            if (gene != null)
            {
                pawn.genes.RemoveGene(gene);
            }
        }
        if (chosenGenes != null)
        {
            foreach (var gene in chosenGenes)
            {
                var gene2 = pawn.genes.GetGene(gene);
                if (gene2 != null)
                {
                    pawn.genes.RemoveGene(gene2);
                }
            }   
        }

        if (chosenTrait != null)
        {
            var trait = pawn.story.traits.GetTrait(chosenTrait);
            if (trait != null)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
        }
        if (!chosenTraits.NullOrEmpty())
        {
            foreach (var traitPair in chosenTraits)
            {
                var trait = pawn.story.traits.GetTrait(traitPair.Key);
                if (trait != null)
                {
                    pawn.story.traits.RemoveTrait(trait);
                }
            }
        }
    }

    private void SelectTraitToGive()
    {
        var defMod = def.GetModExtension<DefModExtension_AddRandomTraitByWeight>();

        var random = new Random();
           
        if (defMod == null || random.Next(0, 100) > defMod.chanceToGrantTrait)
        {
            return;
        }
        
        var possibleTraits = defMod.possibleTraitsToGive.Where(g => !pawn.story.traits.HasTrait(g.traitDef, g.degree)).ToList();
        if (possibleTraits.NullOrEmpty())
        {
            return;
        }
        
        var weightedSelection = new WeightedSelection<TraitData>();
        foreach (var trait in possibleTraits)
        {
            weightedSelection.AddEntry(new TraitData(trait.traitDef, trait.degree), trait.weight);
        }

        if (defMod.amountToGive == 1)
        {
            var result = weightedSelection.GetRandom();
        
            chosenTrait = result.traitDef;
            chosenTraitDegree = result.degree;
        }
        else
        {
            for (var i = 0; i < defMod.amountToGive; i++)
            {
                var result = weightedSelection.GetRandomUnique();
                chosenTraits.Add(result.traitDef, result.degree);
            }
        }
    }

    private void SelectGeneToGive()
    {
        var defMod = def.GetModExtension<DefModExtension_AddRandomGeneByWeight>();

        var random = new Random();

        if (defMod == null || random.Next(0, 100) > defMod.chanceToGrantGene)
        {
            return;
        }
        
        var possibleGenes = defMod.possibleGenesToGive.Where(g => !pawn.genes.HasActiveGene(g.Key)).ToList();
        if (possibleGenes.NullOrEmpty())
        {
            return;
        }
        
        var weightedSelection = new WeightedSelection<GeneDef>();
            
        foreach (var gene in possibleGenes)
        {
            weightedSelection.AddEntry(gene.Key, gene.Value);
        }
            
        if (defMod.amountToGive == 1)
        {
            chosenGene = weightedSelection.GetRandom();
        }
        else
        {
            for (var i = 0; i < defMod.amountToGive; i++)
            {
                var result = weightedSelection.GetRandomUnique();
                chosenGenes.Add(result);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref chosenGene, "chosenGene");
        Scribe_Defs.Look(ref chosenTrait, "chosenTrait");
        Scribe_Collections.Look(ref chosenGenes, "chosenGenes");
        Scribe_Collections.Look(ref chosenTraits, "chosenTraits");
        Scribe_Values.Look(ref chosenTraitDegree, "chosenTraitDegree", 0);
    }
}