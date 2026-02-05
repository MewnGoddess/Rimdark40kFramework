using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class CompWeaponDecoration : CompGraphicParent
{
    private Dictionary<WeaponDecorationDef, ExtraDecorationSettings> originalWeaponDecorations = new ();
    private Dictionary<WeaponDecorationDef, ExtraDecorationSettings> weaponDecorations = new ();

    public Dictionary<WeaponDecorationDef, ExtraDecorationSettings> WeaponDecorations => weaponDecorations;
    
    public CompMultiColor MultiColor => parent.GetComp<CompMultiColor>();
    
    public void AddOrRemoveDecoration(WeaponDecorationDef decoration)
    {
        if (!weaponDecorations.Remove(decoration))
        {
            weaponDecorations.Add(decoration, new ExtraDecorationSettings());
            SetDefaultColors(decoration);
        }

        Notify_GraphicChanged();
    }
    
    public void SetDefaultColors(WeaponDecorationDef decoration)
    {
        weaponDecorations[decoration].Color = decoration.defaultColour ?? (decoration.useWeaponColorAsDefault ? MultiColor?.DrawColor ?? parent.DrawColor : Color.white);
        weaponDecorations[decoration].ColorTwo = decoration.defaultColourTwo ?? (decoration.useWeaponColorAsDefault ? MultiColor?.DrawColorTwo ?? parent.DrawColorTwo : Color.white);
        weaponDecorations[decoration].ColorThree = decoration.defaultColourThree ?? (decoration.useWeaponColorAsDefault ? MultiColor?.DrawColorThree ?? parent.DrawColorTwo : Color.white);
        Notify_GraphicChanged();
    }
    
    public bool recacheGraphics = true;
    
    private Dictionary<WeaponDecorationDef, Graphic> cachedGraphics = [];
    public Dictionary<WeaponDecorationDef, Graphic> Graphics => cachedGraphics;

    public void RecacheGraphics()
    {
        recacheGraphics = false;
        cachedGraphics = [];
        var sortedGraphics = WeaponDecorations.Keys.ToList();
        sortedGraphics.SortBy(deco => deco.layerPlacement);
        foreach (var weaponDecoration in sortedGraphics)
        {
            Graphic graphic;
            if (weaponDecoration.colorAmount > 2)
            {
                graphic = MultiColorUtils.GetGraphic<Graphic_Single>(
                    weaponDecoration.drawnTextureIconPath, 
                    Core40kDefOf.BEWH_CutoutThreeColor.Shader, 
                    weaponDecoration.drawSize, 
                    weaponDecorations[weaponDecoration].Color, 
                    weaponDecorations[weaponDecoration].ColorTwo, 
                    weaponDecorations[weaponDecoration].ColorThree, 
                    null,
                    weaponDecoration.useMask ? weaponDecoration.defaultMask.maskPath : null);
            }
            else
            {
                graphic = GraphicDatabase.Get<Graphic_Single>(
                    weaponDecoration.drawnTextureIconPath, 
                    weaponDecoration.shaderType.Shader ?? ShaderTypeDefOf.Cutout.Shader, 
                    weaponDecoration.drawSize, 
                    weaponDecorations[weaponDecoration].Color, 
                    weaponDecorations[weaponDecoration].ColorTwo, 
                    null,
                    weaponDecoration.useMask ? weaponDecoration.defaultMask.maskPath : null);
            }
            
            cachedGraphics.Add(weaponDecoration, graphic);
        }
    }
    
    public void UpdateDecorationColourOne(WeaponDecorationDef decoration, Color colour)
    {
        weaponDecorations[decoration].Color = colour;
        Notify_GraphicChanged();
    }
    
    public void UpdateDecorationColourTwo(WeaponDecorationDef decoration, Color colour)
    {
        weaponDecorations[decoration].ColorTwo = colour;
        Notify_GraphicChanged();
    }
    
    public void UpdateDecorationColourThree(WeaponDecorationDef decoration, Color colour)
    {
        weaponDecorations[decoration].ColorThree = colour;
        Notify_GraphicChanged();
    }
    
    public void RemoveAllDecorations()
    {
        weaponDecorations = new Dictionary<WeaponDecorationDef, ExtraDecorationSettings>();
        Notify_GraphicChanged();
    }

    public void LoadFromPreset(ExtraDecorationPreset preset)
    {
        foreach (var presetPart in preset.extraDecorationPresetParts)
        {
            var decoDef = Core40kUtils.GetWeaponDecoDefFromString(presetPart.extraDecorationDefs);
            var extraDecorationsSetting = new ExtraDecorationSettings()
            {
                Color = presetPart.colour,
                ColorTwo = presetPart.colourTwo,
                ColorThree = presetPart.colourThree,
            };
            weaponDecorations.Add(decoDef, extraDecorationsSetting);
        }
        Notify_GraphicChanged();
    }
    
    public void LoadFromPreset(WeaponDecorationPresetDef preset)
    {
        foreach (var presetPart in preset.presetData)
        {
            var extraDecorationsSetting = new ExtraDecorationSettings()
            {
                Color = presetPart.colour ?? (presetPart.extraDecorationDef.useArmorColourAsDefault ? parent.DrawColor : Color.white),
                ColorTwo = presetPart.colourTwo ?? Color.white,
                ColorThree = presetPart.colourThree ?? Color.white,
            };
            
            weaponDecorations.Add(presetPart.weaponDecorationDef, extraDecorationsSetting);
        }
        Notify_GraphicChanged();
    }
    
    public override void SetOriginals()
    {
        originalWeaponDecorations = weaponDecorations;
        Notify_GraphicChanged();
    }

    public override void Reset()
    {
        weaponDecorations = originalWeaponDecorations;
        Notify_GraphicChanged();
    }
    
    public override void Notify_GraphicChanged()
    {
        RecacheGraphics();
        base.Notify_GraphicChanged();
    }
    
    public void RemoveInvalidDecorations(Pawn pawn)
    {
        var toRemove = new List<WeaponDecorationDef>();
        foreach (var weaponDecoration in weaponDecorations)
        {
            if (!weaponDecoration.Key.HasRequirements(pawn, out _))
            {
                toRemove.Add(weaponDecoration.Key);
            }
        }
        foreach (var weaponDecorationDef in toRemove)
        {
            weaponDecorations.Remove(weaponDecorationDef);
        }
    }
    
    public override void Notify_Equipped(Pawn pawn)
    {
        RemoveInvalidDecorations(pawn);
        Notify_GraphicChanged();
        base.Notify_Equipped(pawn);
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref originalWeaponDecorations, "originalWeaponDecorations");
        Scribe_Collections.Look(ref weaponDecorations, "weaponDecorations");
    }
}