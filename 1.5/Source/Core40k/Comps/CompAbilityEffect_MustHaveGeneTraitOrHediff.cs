using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k
{
    public class CompAbilityEffect_MustHaveGene : CompAbilityEffect
    {
        public CompProperties_MustHaveGene Props => (CompProperties_MustHaveGene)props;
        
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            base.Valid(target, throwMessages);

            return target.Pawn.genes != null && target.Pawn.genes.HasActiveGene(Props.geneDef);
        }
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            base.ExtraLabelMouseAttachment(target);
            if (target.Pawn.genes == null || !target.Pawn.genes.HasActiveGene(Props.geneDef))
            {
                return "BEWH.PawnDoesNotHaveRequiredGene".Translate(target.Pawn, Props.geneDef.label);
            }
            
            return null;
        }
    }
}