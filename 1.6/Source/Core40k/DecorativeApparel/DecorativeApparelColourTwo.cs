using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class DecorativeApparelColourTwo : ApparelColourTwo
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
        SetInitialColours(defMod.defaultPrimaryColor ?? DrawColor, defMod.defaultSecondaryColor ?? DrawColorTwo);
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
            var extraDecorationsSetting = new ExtraDecorationSettings()
            {
                Flipped = false,
                Color = decoration.useArmorColourAsDefault ? DrawColor : decoration.defaultColour,
            };
            
            extraDecorations.Add(decoration, extraDecorationsSetting);
        }
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
            };
            
            extraDecorations.Add(presetPart.extraDecorationDef, extraDecorationsSetting);
        }
    }
        
    public void UpdateDecorationColour(ExtraDecorationDef decoration, Color colour)
    {
        extraDecorations[decoration].Color = colour;
        Notify_ColorChanged();
    }

    public override void SetOriginals()
    {
        originalExtraDecorations = extraDecorations;
        base.SetOriginals();
    }

    public override void Reset()
    {
        extraDecorations = originalExtraDecorations;
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