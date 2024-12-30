using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Core40k
{
    public class GameComponent_SavedPresets : GameComponent
    {
        public List<ColourPreset> colourPresets = new List<ColourPreset>();
        
        public List<ColourPresetDef> colourPresetDefs = new List<ColourPresetDef>();

        public GameComponent_SavedPresets(Game game)
        {
        }

        public bool AddPreset(ColourPreset preset)
        {
            if (Enumerable.Any(colourPresets, cPreset => cPreset.name == preset.name))
            {
                return false;
            }
            
            colourPresets.Add(preset);
            colourPresetDefs.Add(PresetToDef(preset));
            return true;
        }

        public void UpdatePreset(ColourPresetDef preset, Color primaryColour, Color secondaryColour)
        {
            var existingPreset = colourPresets.Find(cPreset => cPreset.name == preset.label);
            existingPreset.primaryColour = primaryColour;
            existingPreset.secondaryColour = secondaryColour;
            
            var existingPresetDef = colourPresetDefs.Find(cPreset => cPreset.label == preset.label);
            existingPresetDef.primaryColour = primaryColour;
            existingPresetDef.secondaryColour = secondaryColour;
        }
        
        public void RemovePreset(ColourPreset preset)
        {
            if (!colourPresets.Contains(preset))
            {
                return;
            }
            
            colourPresets.Remove(preset);
            var presetDef = colourPresetDefs.Find(cPreset => cPreset.label == preset.name);
            colourPresetDefs.Remove(presetDef);
        }
        
        public void RemovePreset(ColourPresetDef presetDef)
        {
            if (!colourPresetDefs.Contains(presetDef))
            {
                return;
            }
            
            
            var preset = colourPresets.Find(cPreset => cPreset.name == presetDef.label);
            colourPresets.Remove(preset);
            colourPresetDefs.Remove(presetDef);
        }

        private static ColourPresetDef PresetToDef(ColourPreset preset)
        {
            var cPreset = new ColourPresetDef
            {
                defName = "BEWH_" + preset.name,
                label = preset.name,
                primaryColour = preset.primaryColour,
                secondaryColour = preset.secondaryColour,
            };

            return cPreset;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref colourPresets, "colourPresets");

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }
            
            foreach (var colourPreset in colourPresets)
            {
                colourPresetDefs.Add(PresetToDef(colourPreset));
            }
        }
    }
}