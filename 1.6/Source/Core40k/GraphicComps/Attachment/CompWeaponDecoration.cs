using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class CompWeaponDecoration : CompGraphicParent
{
    private static GameComponent_CoreUtils coreUtils;
    private static GameComponent_CoreUtils CoreUtils => coreUtils ??= Current.Game.GetComponent<GameComponent_CoreUtils>();
    
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
        originalWeaponDecorations = new Dictionary<WeaponDecorationDef, ExtraDecorationSettings>();
        originalWeaponDecorations.AddRange(weaponDecorations);
        Notify_GraphicChanged();
        base.SetOriginals();
    }

    public override void Reset()
    {
        weaponDecorations = new Dictionary<WeaponDecorationDef, ExtraDecorationSettings>();
        weaponDecorations.AddRange(originalWeaponDecorations);
        cachedGraphics = [];
        Notify_GraphicChanged();
        base.Reset();
    }
    
    public override void Notify_GraphicChanged()
    {
        RecacheGraphics();
        cachedStatOffset = new Dictionary<StatDef, float>();
        cachedStatFactor = new Dictionary<StatDef, float>();
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
    
    public void RemoveDecorationsIncompatibleWithAlternate(AlternateBaseFormDef alternateBaseFormDef)
    {
        var toRemove = new List<WeaponDecorationDef>();
        foreach (var weaponDecoration in weaponDecorations)
        {
            if (alternateBaseFormDef == null && weaponDecoration.Key.isIncompatibleWithBaseTexture)
            {
                toRemove.Add(weaponDecoration.Key);
            }
            else if (alternateBaseFormDef != null && alternateBaseFormDef.incompatibleWeaponDecorations.Contains(weaponDecoration.Key))
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

        if (pawn != null)
        {
            cachedStatOffset = new Dictionary<StatDef, float>();
            cachedStatFactor = new Dictionary<StatDef, float>();
            if (CoreUtils.cachedDecoratives.TryGetValue(pawn, out var decoratives))
            {
                decoratives.weapon = parent;
            }
            else
            {
                var cachedDecoratives = new GameComponent_CoreUtils.CachedDecoratives
                {
                    weapon = parent,
                };
                CoreUtils.cachedDecoratives.Add(pawn, cachedDecoratives);
            }
        }
        
        base.Notify_Equipped(pawn);
    }
    
    public override void Notify_Unequipped(Pawn pawn)
    {
        if (pawn != null)
        {
            if (CoreUtils.cachedDecoratives.TryGetValue(pawn, out var decoratives))
            {
                decoratives.weapon = null;
                cachedStatOffset = new Dictionary<StatDef, float>();
                cachedStatFactor = new Dictionary<StatDef, float>();
            }
        }
        
        base.Notify_Unequipped(pawn);
    }
    
    private Dictionary<StatDef, float> cachedStatOffset = new Dictionary<StatDef, float>();
    public Dictionary<StatDef, float> CachedStatOffset => cachedStatOffset;
    
    private Dictionary<StatDef, float> cachedStatFactor = new Dictionary<StatDef, float>();
    public Dictionary<StatDef, float> CachedStatFactor => cachedStatFactor;
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref originalWeaponDecorations, "originalWeaponDecorations");
        Scribe_Collections.Look(ref weaponDecorations, "weaponDecorations");
    }
}