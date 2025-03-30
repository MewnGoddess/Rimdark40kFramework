using System.Collections.Generic;
using Core40k;
using UnityEngine;
using Verse;

namespace Genes40k
{
    public class DecorativeApparelColourTwo : ApparelColourTwo
    {
        private Dictionary<ExtraDecorationDef, bool> originalExtraDecorations = new Dictionary<ExtraDecorationDef, bool>();
        private Dictionary<ExtraDecorationDef, bool> extraDecorations = new Dictionary<ExtraDecorationDef, bool>();
        
        private Dictionary<ExtraDecorationDef, Color> originalExtraDecorationsColours = new Dictionary<ExtraDecorationDef, Color>();
        private Dictionary<ExtraDecorationDef, Color> extraDecorationsColours = new Dictionary<ExtraDecorationDef, Color>();

        public Dictionary<ExtraDecorationDef, bool> ExtraDecorationDefs => extraDecorations;
        
        public Dictionary<ExtraDecorationDef, Color> ExtraDecorationColours => extraDecorationsColours;

        public void AddOrRemoveDecoration(ExtraDecorationDef decoration)
        {
            if (extraDecorations.ContainsKey(decoration) && (extraDecorations[decoration] || !decoration.flipable))
            {
                extraDecorations.Remove(decoration);
                extraDecorationsColours.Remove(decoration);
            }
            else if (extraDecorations.ContainsKey(decoration))
            {
                extraDecorations[decoration] = true;
            }
            else
            {
                extraDecorations.Add(decoration, false);
                var color = decoration.useArmorColourAsDefault ? DrawColor : decoration.defaultColour;
                extraDecorationsColours.Add(decoration, color);
            }
            Notify_ColorChanged();
        }

        public void RemoveAllDecorations()
        {
            extraDecorations = new Dictionary<ExtraDecorationDef, bool>();
            extraDecorationsColours = new Dictionary<ExtraDecorationDef, Color>();
            Notify_ColorChanged();
        }

        public void UpdateDecorationColour(ExtraDecorationDef decoration, Color colour)
        {
            extraDecorationsColours[decoration] = colour;
            Notify_ColorChanged();
        }

        public void UpdateAllDecorationColours(Color colour)
        {
            var newExtraDecorationsColours = new Dictionary<ExtraDecorationDef, Color>();
            
            foreach (var extraDecoration in extraDecorationsColours)
            {
                newExtraDecorationsColours.Add(extraDecoration.Key, colour);
            }
            
            extraDecorationsColours = newExtraDecorationsColours;
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
            
            Scribe_Collections.Look(ref extraDecorationsColours, "extraDecorationsColours");
            Scribe_Collections.Look(ref originalExtraDecorationsColours, "originalExtraDecorationsColours");
            base.ExposeData();
        }
    }
}