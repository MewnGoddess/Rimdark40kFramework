using Verse;

namespace Core40k;

public class Verb_ShootChangeableBullets : Verb_Shoot
{
    [Unsaved(false)]
    private Comp_AmmoChanger cachedAmmoChangerComp;
    private Comp_AmmoChanger AmmoChangerComp => cachedAmmoChangerComp ??= EquipmentSource.TryGetComp<Comp_AmmoChanger>();
    
    public override ThingDef Projectile => AmmoChangerComp.CurrentlySelectedProjectile ?? verbProps.defaultProjectile;
}