using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;


namespace Core40k
{
    public class Core40kModSettings : ModSettings
    {
        public bool alwaysShowRankTab = false;

        public bool showAllRankCategories = false;
        
        private List<ColourPreset> colourPresets = new List<ColourPreset>();
        public List<ColourPreset> ColourPresets => colourPresets;
        
        private List<ExtraDecorationPreset> extraDecorationPresets = new List<ExtraDecorationPreset>();
        public List<ExtraDecorationPreset> ExtraDecorationPresets => extraDecorationPresets;

        //Colour Preset
        public bool AddPreset(ColourPreset preset)
        {
            if (Enumerable.Any(colourPresets, cPreset => cPreset.name == preset.name))
            {
                return false;
            }
            
            colourPresets.Add(preset);
            
            Mod.WriteSettings();
            return true;
        }
        public void UpdatePreset(ColourPreset preset, Color primaryColour, Color secondaryColour)
        {
            var existingPreset = colourPresets.Find(cPreset => cPreset.name == preset.name);
            existingPreset.primaryColour = primaryColour;
            existingPreset.secondaryColour = secondaryColour;
            Mod.WriteSettings();
        }
        public void RemovePreset(ColourPreset preset)
        {
            if (!colourPresets.Contains(preset))
            {
                return;
            }
            
            colourPresets.Remove(preset);
            Mod.WriteSettings();
        }
        
        //Extra Decoration Preset
        public bool AddPreset(ExtraDecorationPreset preset)
        {
            if (Enumerable.Any(extraDecorationPresets, cPreset => cPreset.name == preset.name))
            {
                return false;
            }
            
            extraDecorationPresets.Add(preset);
            
            Mod.WriteSettings();
            return true;
        }
        public void UpdatePreset(ExtraDecorationPreset preset, ExtraDecorationPreset newPreset)
        {
            if (!extraDecorationPresets.Contains(preset))
            {
                return;
            }
            
            var indexOf = extraDecorationPresets.IndexOf(preset);
            extraDecorationPresets[indexOf] = newPreset;
            
            Mod.WriteSettings();
        }
        public void RemovePreset(ExtraDecorationPreset preset)
        {
            if (!extraDecorationPresets.Contains(preset))
            {
                return;
            }
            
            extraDecorationPresets.Remove(preset);
            Mod.WriteSettings();
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref alwaysShowRankTab, "alwaysShowRankTab", false);
            Scribe_Values.Look(ref showAllRankCategories, "showAllRankCategories", false);
            Scribe_Collections.Look(ref colourPresets, "colourPresets");
            Scribe_Collections.Look(ref extraDecorationPresets, "extraDecorationPresets");
            base.ExposeData();
        }
    }
}