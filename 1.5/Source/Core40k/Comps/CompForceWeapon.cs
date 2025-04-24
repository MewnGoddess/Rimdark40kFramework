using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Core40k;

public class CompForceWeapon : ThingComp
{
    public CompProperties_ForceWeapon Props => (CompProperties_ForceWeapon)props;

    private float statValue = -1;

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        CalculateExtraDamage(pawn);
    }

    public override void Notify_UsedWeapon(Pawn pawn)
    {
        if (pawn.GetStatValue(Props.scalingStat) != statValue)
        {
            CalculateExtraDamage(pawn);
        }
    }

    private void CalculateExtraDamage(Pawn pawn)
    {
        if (pawn == null || pawn.GetStatValue(Props.scalingStat) <= 0)
        {
            return;
        }

        statValue = pawn.GetStatValue(Props.scalingStat);

        var cachedDamageValue = statValue * Props.damageScalingFactor;
        var cachedPenValue = Props.flatPen;

        if (Props.scalesPen)
        {
            cachedPenValue = pawn.GetStatValue(Props.scalingStat) * Props.penScaleFactor;
        }

        if (parent.TryGetComp<CompEquippable>().Tools == null) return;
            
        foreach (var tool in parent.TryGetComp<CompEquippable>().Tools)
        {
            if (tool.capacities.Intersect(Props.capacitiesToApplyOn).EnumerableNullOrEmpty()) continue;
                
            var extraDamage = new ExtraDamage
            {
                def = Props.damageDef,
                amount = cachedDamageValue,
                armorPenetration = cachedPenValue
            };
                    
            if (tool.extraMeleeDamages.NullOrEmpty())
            {
                tool.extraMeleeDamages = new List<ExtraDamage>();
            }
            tool.extraMeleeDamages.Add(extraDamage);
        }
    }
}