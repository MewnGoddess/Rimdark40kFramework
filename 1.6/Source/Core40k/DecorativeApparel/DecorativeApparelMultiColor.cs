using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class DecorativeApparelMultiColor : ApparelMultiColor
{
    private Dictionary<ExtraDecorationDef, ExtraDecorationSettings> originalExtraDecorations = new ();
    private Dictionary<ExtraDecorationDef, ExtraDecorationSettings> extraDecorations = new ();
    
    public Dictionary<ExtraDecorationDef, ExtraDecorationSettings> ExtraDecorations => extraDecorations;

    private bool extraDecoSetup = false;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        
        if (!def.HasModExtension<DefModExtension_StandardDecorations>() || extraDecoSetup)
        {
            return;
        }
        
        extraDecoSetup = true;
        var defMod = def.GetModExtension<DefModExtension_StandardDecorations>();
        SetInitialColours(defMod.defaultPrimaryColor ?? DrawColor, defMod.defaultSecondaryColor ?? DrawColorTwo, defMod.defaultTertiaryColor ?? DrawColorThree);
        foreach (var extraDecoration in defMod.extraDecorations)
        {
            AddOrRemoveDecoration(extraDecoration);
        }
    }
    
    public void AddOrRemoveDecoration(ExtraDecorationDef decoration)
    {
        if (extraDecorations.ContainsKey(decoration) && (extraDecorations[decoration].Flipped || !decoration.flipable))
        {
            extraDecorations.Remove(decoration);
        }
        else if (extraDecorations.TryGetValue(decoration, out var setting))
        {
            setting.Flipped = true;
        }
        else
        {
            extraDecorations.Add(decoration, new ExtraDecorationSettings());
            SetDefaultColors(decoration);
        }
        Notify_ColorChanged();
    }

    public void SetDefaultColors(ExtraDecorationDef decoration)
    {
        extraDecorations[decoration].Color = decoration.defaultColour ?? (decoration.useArmorColourAsDefault ? DrawColor : Color.white);
        extraDecorations[decoration].ColorTwo = decoration.defaultColourTwo ?? (decoration.useArmorColourAsDefault ? DrawColorTwo : Color.white);
        extraDecorations[decoration].ColorThree = decoration.defaultColourThree ?? (decoration.useArmorColourAsDefault ? DrawColorThree : Color.white);
        extraDecorations[decoration].maskDef = decoration.defaultMask;
        Notify_ColorChanged();
    }
    
    public void SetArmorColors(ExtraDecorationDef decoration)
    {
        extraDecorations[decoration].Color = DrawColor;
        extraDecorations[decoration].ColorTwo = DrawColorTwo;
        extraDecorations[decoration].ColorThree = DrawColorThree;
        Notify_ColorChanged();
    }

    public void RemoveAllDecorations()
    {
        extraDecorations = new Dictionary<ExtraDecorationDef, ExtraDecorationSettings>();
        Notify_ColorChanged();
    }

    public void LoadFromPreset(ExtraDecorationPreset preset)
    {
        foreach (var presetPart in preset.extraDecorationPresetParts)
        {
            var decoDef = Core40kUtils.GetDefFromString(presetPart.extraDecorationDefs);
            var extraDecorationsSetting = new ExtraDecorationSettings()
            {
                Flipped = presetPart.flipped,
                Color = presetPart.colour,
                ColorTwo = presetPart.colourTwo,
                ColorThree = presetPart.colourThree,
            };
            
            extraDecorations.Add(decoDef, extraDecorationsSetting);
        }
    }
    
    public void LoadFromPreset(ExtraDecorationPresetDef preset)
    {
        foreach (var presetPart in preset.presetData)
        {
            var extraDecorationsSetting = new ExtraDecorationSettings()
            {
                Flipped = presetPart.flipped,
                Color = presetPart.colour ?? (presetPart.extraDecorationDef.useArmorColourAsDefault ? DrawColor : Color.white),
                ColorTwo = presetPart.colourTwo ?? Color.white,
                ColorThree = presetPart.colourThree ?? Color.white
            };
            
            extraDecorations.Add(presetPart.extraDecorationDef, extraDecorationsSetting);
        }
    }
        
    public void UpdateDecorationColourOne(ExtraDecorationDef decoration, Color colour)
    {
        extraDecorations[decoration].Color = colour;
        Notify_ColorChanged();
    }
    
    public void UpdateDecorationColourTwo(ExtraDecorationDef decoration, Color colour)
    {
        extraDecorations[decoration].ColorTwo = colour;
        Notify_ColorChanged();
    }
    
    public void UpdateDecorationColourThree(ExtraDecorationDef decoration, Color colour)
    {
        extraDecorations[decoration].ColorThree = colour;
        Notify_ColorChanged();
    }

    public void UpdateDecorationMask(ExtraDecorationDef decoration, MaskDef maskDef)
    {
        extraDecorations[decoration].maskDef = maskDef.setsNull ? null : maskDef;
        Notify_ColorChanged();
    }

    public override void SetOriginals()
    {
        originalExtraDecorations = new Dictionary<ExtraDecorationDef, ExtraDecorationSettings>();
        originalExtraDecorations.AddRange(extraDecorations);
        base.SetOriginals();
    }

    public override void Reset()
    {
        extraDecorations = new Dictionary<ExtraDecorationDef, ExtraDecorationSettings>();
        extraDecorations.AddRange(originalExtraDecorations);
        base.Reset();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref extraDecorations, "extraDecorations");
        Scribe_Collections.Look(ref originalExtraDecorations, "originalExtraDecorations");
        Scribe_Values.Look(ref extraDecoSetup, "extraDecoSetup");
        base.ExposeData();
    }
}