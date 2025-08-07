using RimWorld;
using Verse;

namespace Core40k;

public class Verb_ShootChangeableBullets : Verb_Shoot
{
    [Unsaved(false)]
    private Comp_AmmoChanger cachedAmmoChangerComp;
    private Comp_AmmoChanger AmmoChangerComp => cachedAmmoChangerComp ??= EquipmentSource.TryGetComp<Comp_AmmoChanger>();
    
    public override ThingDef Projectile => AmmoChangerComp.CurrentlySelectedProjectile ?? verbProps.defaultProjectile;

    public DefModExtension_AmmoChanger DefModExtensionAmmoChanger => AmmoChangerComp.CurrentlySelectedProjectile.GetModExtension<DefModExtension_AmmoChanger>();

    protected override int ShotsPerBurst => DefModExtensionAmmoChanger?.shotsPerBurst ?? base.ShotsPerBurst;
    public override float EffectiveRange => (DefModExtensionAmmoChanger?.effectiveRange ?? base.EffectiveRange) * (EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier) ?? 1f);

    public override float WarmupTime => (DefModExtensionAmmoChanger?.warmupTime ?? base.WarmupTime) * (EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_WarmupMultiplier) ?? 1f);
}