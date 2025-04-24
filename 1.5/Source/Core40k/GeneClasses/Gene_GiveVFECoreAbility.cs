using Verse;
using VFECore.Abilities;

namespace Core40k;

public class Gene_GiveVFECoreAbility : Gene
{
    public override void PostAdd()
    {
        var comp = pawn.GetComp<CompAbilities>();
        if (comp != null)
        {
            if (def.HasModExtension<DefModExtension_GivesVFEAbility>())
            {
                var defModExtension = def.GetModExtension<DefModExtension_GivesVFEAbility>();
                foreach (var abilityDef in defModExtension.abilityDefs)
                {
                    comp.GiveAbility(abilityDef);
                }
            }
        }

        base.PostAdd();
    }

    public override void PostRemove()
    {
        var comp = pawn.GetComp<CompAbilities>();
        if (comp != null)
        {
            if (def.HasModExtension<DefModExtension_GivesVFEAbility>())
            {
                var defModExtension = def.GetModExtension<DefModExtension_GivesVFEAbility>();
                for (var i = 0; i < comp.LearnedAbilities.Count; i++)
                {
                    foreach (var abilityDef in defModExtension.abilityDefs)
                    {
                        if (comp.LearnedAbilities[i].def == abilityDef)
                        {
                            comp.LearnedAbilities.RemoveAt(i);
                        }
                    }
                }
            }
        }

        base.PostRemove();
    }
}