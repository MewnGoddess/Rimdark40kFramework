using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class Gene_AddRandomGeneAndOrTraitByWeight : Gene
    {
        private static GeneDef chosenGene;

        private static TraitDef chosenTrait;
        private static int chosenTraitDegree;

        public override void PostAdd()
        {
            base.PostAdd();

            SelectGeneToGive();
            SelectTraitToGive();

            if (chosenGene != null)
            {
                pawn.genes.AddGene(chosenGene, true);
            }

            if (chosenTrait == null) return;
            
            var trait = new Trait(chosenTrait, chosenTraitDegree);
            pawn.story.traits.GainTrait(trait);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (chosenGene != null)
            {
                var gene = pawn.genes.GetGene(chosenGene);
                if (gene != null)
                {
                    pawn.genes.RemoveGene(gene);
                }
            }

            if (chosenTrait == null) return;
            
            var trait = pawn.story.traits.GetTrait(chosenTrait);
            if (trait != null)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
        }

        public virtual void SelectTraitToGive()
        {
            var defMod = def.GetModExtension<DefModExtension_AddRandomTraitByWeight>();

            var random = new Random();
           
            if (random.Next(0, 100) < defMod.chanceToGrantTrait)
            {
                return;
            }

            var weightedSelection = new WeightedSelection<Dictionary<TraitDef, int>>();
            var possibleTraits = defMod.possibleTraitsToGive.Where(g => !pawn.story.traits.HasTrait(g.Key.Keys.First(), g.Key.Values.First()));

            if (possibleTraits.EnumerableNullOrEmpty())
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

        public virtual void SelectGeneToGive()
        {
            var defMod = def.GetModExtension<DefModExtension_AddRandomGeneByWeight>();

            var random = new Random();

            if (random.Next(0, 100) < defMod.chanceToGrantGene)
            {
                return;
            }

            var weightedSelection = new WeightedSelection<GeneDef>();
            var possibleGenes = defMod.possibleGenesToGive.Where(g => !pawn.genes.HasActiveGene(g.Key));

            if (possibleGenes.EnumerableNullOrEmpty())
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
            Scribe_Defs.Look(ref chosenGene, "chosenGene");
            Scribe_Defs.Look(ref chosenTrait, "chosenTrait");
        }
    }
}   