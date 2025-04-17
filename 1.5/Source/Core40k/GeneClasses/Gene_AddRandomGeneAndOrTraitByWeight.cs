using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class Gene_AddRandomGeneAndOrTraitByWeight : Gene
    {
        private static GeneDef chosenGene = null;

        private static TraitDef chosenTrait = null;
        private static int chosenTraitDegree = 0;

        public override void PostMake()
        {
            SelectGeneToGive();
            SelectTraitToGive();
        }
        public override void PostAdd()
        {
            AddSelectedTraitAndGene();
            
            base.PostAdd();
        }
        public override void PostRemove()
        {
            RemoveSelectedTraitAndGene();
            base.PostRemove();
        }
            
        private void AddSelectedTraitAndGene()
        {
            if (chosenGene != null)
            {
                pawn.genes.AddGene(chosenGene, true);
            }

            if (chosenTrait != null)
            {
                var trait = new Trait(chosenTrait, chosenTraitDegree);
                pawn.story.traits.GainTrait(trait);
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

            if (chosenTrait != null)
            {
                var trait = pawn.story.traits.GetTrait(chosenTrait);
                if (trait != null)
                {
                    pawn.story.traits.RemoveTrait(trait);
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

            var weightedSelection = new WeightedSelection<Dictionary<TraitDef, int>>();
            var possibleTraits = defMod.possibleTraitsToGive.Where(g => !pawn.story.traits.HasTrait(g.Key.Keys.First(), g.Key.Values.First())).ToList();

            if (possibleTraits.NullOrEmpty())
            {
                return;
            }

            foreach (var trait in possibleTraits)
            {
                weightedSelection.AddEntry(trait.Key, trait.Value);
            }

            var result = weightedSelection.GetRandom();

            chosenTrait = result.Keys.First();

            chosenTraitDegree = result.Values.First();
        }

        private void SelectGeneToGive()
        {
            var defMod = def.GetModExtension<DefModExtension_AddRandomGeneByWeight>();

            var random = new Random();

            if (defMod == null || random.Next(0, 100) > defMod.chanceToGrantGene)
            {
                return;
            }
            
            var weightedSelection = new WeightedSelection<GeneDef>();
            var possibleGenes = defMod.possibleGenesToGive.Where(g => !pawn.genes.HasActiveGene(g.Key)).ToList();
            if (possibleGenes.NullOrEmpty())
            {
                return;
            }
            
            foreach (var gene in possibleGenes)
            {
                weightedSelection.AddEntry(gene.Key, gene.Value);
            }
            
            chosenGene = weightedSelection.GetRandom();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref chosenGene, "chosenGene");
            Scribe_Defs.Look(ref chosenTrait, "chosenTrait");
            Scribe_Values.Look(ref chosenTraitDegree, "chosenTraitDegree", 0);
        }
    }
}   