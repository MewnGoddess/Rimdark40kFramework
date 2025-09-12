using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k;

public class Gene_AddRandomGeneAndOrTraitByWeight : Gene
{
    private GeneDef chosenGene = null;
    private List<GeneDef> chosenGenes = new List<GeneDef>();

    private TraitDef chosenTrait = null;
    private int chosenTraitDegree = 0;
    private Dictionary<TraitDef, int> chosenTraits = new Dictionary<TraitDef, int>();
    
    private DefModExtension_AddRandomGeneByWeight GeneDefMod => def.GetModExtension<DefModExtension_AddRandomGeneByWeight>();
    private DefModExtension_AddRandomTraitByWeight TraitDefMod => def.GetModExtension<DefModExtension_AddRandomTraitByWeight>();
    
    public override void PostMake()
    {
        base.PostMake();
        if (GeneDefMod != null)
        {
            SelectGeneToGive();
        }

        if (TraitDefMod != null)
        {
            SelectTraitToGive();
        }
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
        if (GeneDefMod != null)
        {
            if (chosenGene != null)
            {
                pawn.genes.AddGene(chosenGene, GeneDefMod.addAsXenogene);
            }
            
            if (!chosenGenes.NullOrEmpty())
            {
                foreach (var gene in chosenGenes)
                {
                    pawn.genes.AddGene(gene, GeneDefMod.addAsXenogene);
                }
            }
        }

        if (TraitDefMod != null)
        {
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
    }
        
    private void RemoveSelectedTraitAndGene()
    {
        if (GeneDefMod != null)
        {
            if (chosenGene != null)
            {
                var gene = pawn.genes.GetGene(chosenGene);
                if (gene != null)
                {
                    pawn.genes.RemoveGene(gene);
                }
            }
            
            if (!chosenGenes.NullOrEmpty())
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
        }
        
        if (TraitDefMod != null)
        {
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
    }

    private void SelectTraitToGive()
    {
        var random = new Random();
           
        if (random.Next(0, 100) > TraitDefMod.chanceToGrantTrait)
        {
            return;
        }
        
        var possibleTraits = TraitDefMod.possibleTraitsToGive.Where(g => !pawn.story.traits.HasTrait(g.traitDef, g.degree)).ToList();
        if (possibleTraits.NullOrEmpty())
        {
            return;
        }
        
        var weightedSelection = new WeightedSelection<TraitData>();
        foreach (var trait in possibleTraits)
        {
            weightedSelection.AddEntry(new TraitData(trait.traitDef, trait.degree), trait.weight);
        }

        if (TraitDefMod.amountToGive == 1)
        {
            var result = weightedSelection.GetRandom();
        
            chosenTrait = result.traitDef;
            chosenTraitDegree = result.degree;
        }
        else if (TraitDefMod.amountToGive == TraitDefMod.possibleTraitsToGive.Count)
        {
            foreach (var traitData in TraitDefMod.possibleTraitsToGive)
            {
                if (chosenTraits.ContainsKey(traitData.traitDef))
                {
                    continue;
                }
                chosenTraits.Add(traitData.traitDef, traitData.degree);
            }
        }
        else
        {
            for (var i = 0; i < TraitDefMod.amountToGive; i++)
            {
                var result = weightedSelection.GetRandomUnique();
                chosenTraits.Add(result.traitDef, result.degree);
            }
        }
    }

    private void SelectGeneToGive()
    {
        var random = new Random();

        if (random.Next(0, 100) > GeneDefMod.chanceToGrantGene)
        {
            return;
        }
        
        var possibleGenes = GeneDefMod.possibleGenesToGive.Where(g => !pawn.genes.HasActiveGene(g.Key)).ToList();
        if (possibleGenes.NullOrEmpty())
        {
            return;
        }

        if (GeneDefMod.skipIfAnyAlreadyExistsOnPawn && possibleGenes.Count < GeneDefMod.possibleGenesToGive.Count)
        {
            return;
        }
        
        var weightedSelection = new WeightedSelection<GeneDef>();
            
        foreach (var gene in possibleGenes)
        {
            weightedSelection.AddEntry(gene.Key, gene.Value);
        }
        if (GeneDefMod.amountToGive == 1)
        {
            chosenGene = weightedSelection.GetRandom();
        }
        else if (GeneDefMod.amountToGive == GeneDefMod.possibleGenesToGive.Count)
        {
            chosenGenes.AddRangeUnique(GeneDefMod.possibleGenesToGive.Select(pair => pair.Key));
        }
        else
        {
            for (var i = 0; i < GeneDefMod.amountToGive; i++)
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