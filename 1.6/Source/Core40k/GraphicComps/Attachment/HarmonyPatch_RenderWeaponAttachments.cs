using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming")]
public static class RenderWeaponAttachments
{
    public static void Postfix(Thing eq, Vector3 drawLoc, float aimAngle)
    {
        if (eq is not ThingWithComps weapon)
        {
            return;
        }

        var decoComp = weapon.GetComp<CompWeaponDecoration>();

        if (decoComp == null)
        {
            return;
        }

        if (decoComp.recacheGraphics)
        {
            decoComp.RecacheGraphics();
        }
        
        DrawAttachments(weapon, decoComp,drawLoc, aimAngle);
    }

    private static void DrawAttachments(ThingWithComps eq, CompWeaponDecoration decoComp, Vector3 drawLoc, float aimAngle)
    {
        var num = aimAngle - 90f;
        Mesh mesh;
        switch (aimAngle)
        {
            case > 20f and < 160f:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
            case > 200f and < 340f:
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
                break;
            default:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
        }
        num %= 360f;
        var compEquippable = eq.TryGetComp<CompEquippable>();
        if (compEquippable != null)
        {
            EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
            drawLoc += drawOffset;
            num += angleOffset;
        }
        
        foreach (var decoCompGraphic in decoComp.Graphics)
        {
            var material = decoCompGraphic.MatSingle;
            var matrix = Matrix4x4.TRS(s: new Vector3(decoCompGraphic.drawSize.x, 0f, decoCompGraphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
    }
}