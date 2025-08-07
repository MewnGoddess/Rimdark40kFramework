using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Core40k;

public class JobDriver_ChangeAmmo : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var pawnManipulation = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
        var manipulation = pawnManipulation < 1 ? pawnManipulation : Math.Max(pawnManipulation / 2, 1);
        var reloadTime = (int)Math.Min(100 / manipulation, 20);
        yield return Toils_General.Wait(reloadTime).WithProgressBarToilDelay(TargetIndex.A);
        yield return Toils_General.Do(delegate
        {
            if (job.targetA.Thing is not ThingWithComps thingWithComps)
            {
                return;
            }
            
            thingWithComps.GetComp<Comp_AmmoChanger>().LoadNextProjectile();
        });
    }
}