using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

[StaticConstructorOnStartup]
public class Gizmo_AmmoChanger : Gizmo
{
    private float Width => 75f;

    private Comp_AmmoChanger compAmmoChanger;
    
    public Gizmo_AmmoChanger(Comp_AmmoChanger compAmmoChanger)
    {
        this.compAmmoChanger = compAmmoChanger;
    }
    
    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        TooltipHandler.TipRegion(rect, "BEWH.Framework.AmmoChanger.GizmoInfo".Translate(compAmmoChanger.CurrentlySelectedProjectile));
        Widgets.DrawWindowBackground(rect);
        
        if (Widgets.ButtonImage(rect, compAmmoChanger.CurrentlySelectedProjectile.uiIcon))
        {
            var list = new List<FloatMenuOption>();
            foreach (var availableProjectile in compAmmoChanger.AvailableProjectiles)
            {
                var menuOption = new FloatMenuOption(availableProjectile.label.CapitalizeFirst(), delegate
                {
                    compAmmoChanger.CurrentlySelectedProjectile = availableProjectile;
                }, Widgets.PlaceholderIconTex, Color.white);
                if (!compAmmoChanger.HasResearchForAmmo(availableProjectile, out var researchDef))
                {
                    menuOption.Disabled = true;
                    menuOption.tooltip = "BEWH.Framework.AmmoChanger.MissingResearch".Translate(researchDef.label.CapitalizeFirst());
                }

                if (availableProjectile == compAmmoChanger.CurrentlySelectedProjectile)
                {
                    menuOption.Disabled = true;
                    menuOption.tooltip = "BEWH.Framework.AmmoChanger.CurrentlyEquipped".Translate();
                }
                list.Add(menuOption);
            }
            if (list.NullOrEmpty())
            {
                var menuOptionNone = new FloatMenuOption("NoneBrackets".Translate(), null);
                list.Add(menuOptionNone);
            }
            
            Find.WindowStack.Add(new FloatMenu(list));
        }
        
        return new GizmoResult(GizmoState.Clear);;
    }

    public override float GetWidth(float maxWidth)
    {
        return Width;
    }
}