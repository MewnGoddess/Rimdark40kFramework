using RimWorld;
using Verse;

namespace Core40k;

public class CompAbilityEffect_GiveHediffAndMentalBreak : CompAbilityEffect_GiveHediff
{
    public CompProperties_AbilityGiveHediffAndMental PropsMental => (CompProperties_AbilityGiveHediffAndMental)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        target.Pawn.mindState.mentalStateHandler.TryStartMentalState(PropsMental.mentalStateDef);
    }
}