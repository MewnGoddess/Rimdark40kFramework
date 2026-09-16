using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Core40k;

/// <summary>
/// A shield field that is dormant until something raises the item's EnergyShieldEnergyMax above
/// zero, so an item can carry the comp permanently and have a fitted upgrade switch it on.
/// Unlike a shield belt it defaults to blocksRangedWeapons false, so the wearer can still shoot.
/// </summary>
[StaticConstructorOnStartup]
public class Comp_ConversionField : CompShield
{
    private bool FieldActive => HasFieldSource && parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax) > 0f;

    /// <summary>
    /// True only while a fitted decoration is what supplies the field. EnergyShieldEnergyMax has no
    /// defaultBaseValue of its own, so a StatDef default of 1 would otherwise give every item with
    /// this comp a full shield; an item meant to carry a field of its own declares the stat instead.
    /// </summary>
    private bool HasFieldSource
    {
        get
        {
            if (parent.def.StatBaseDefined(StatDefOf.EnergyShieldEnergyMax)
                && parent.def.GetStatValueAbstract(StatDefOf.EnergyShieldEnergyMax) > 0f)
            {
                return true;
            }

            foreach (var comp in parent.AllComps)
            {
                if (comp is not CompDecorativeBase decorative)
                {
                    continue;
                }

                foreach (var decoration in decorative.Decorations)
                {
                    if (decoration.Key != null
                        && decoration.Key.statOffsets.GetStatOffsetFromList(StatDefOf.EnergyShieldEnergyMax) > 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public override void CompTick()
    {
        if (!FieldActive)
        {
            energy = 0f;
            ticksToReset = -1;
            return;
        }

        base.CompTick();
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;
        if (!FieldActive)
        {
            return;
        }

        base.PostPreApplyDamage(ref dinfo, out absorbed);
    }

    public override void CompDrawWornExtras()
    {
        if (!FieldActive)
        {
            return;
        }

        base.CompDrawWornExtras();
    }

    public override void PostDraw()
    {
        if (!FieldActive)
        {
            return;
        }

        base.PostDraw();
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        if (!FieldActive)
        {
            yield break;
        }

        foreach (var gizmo in base.CompGetWornGizmosExtra())
        {
            yield return gizmo;
        }
    }

    public override float CompGetSpecialApparelScoreOffset()
    {
        return !FieldActive ? 0f : base.CompGetSpecialApparelScoreOffset();
    }
}
