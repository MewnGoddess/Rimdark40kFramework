using RimWorld;
using Verse;

namespace Core40k;

public class Comp_DisableInactiveGene : CompAbilityEffect
{
    private new CompProperties_DisableInactiveGene Props => (CompProperties_DisableInactiveGene)props;

    public override bool ShouldHideGizmo
    {
        get
        {
            if (Props.geneDef != null && !parent.pawn.genes.HasActiveGene(Props.geneDef))
            {
                return true;
            }

            return false;
        }
    }

    public override bool GizmoDisabled(out string reason)
    {
        if (Props.geneDef != null && !parent.pawn.genes.HasActiveGene(Props.geneDef))
        {
            reason = "BEWH.Framework.Comp.PawnDoesNotHaveRequiredGene".Translate(parent.pawn.LabelShort, Props.geneDef.label.CapitalizeFirst());
            return false;
        }
            
        return base.GizmoDisabled(out reason);
    }
}