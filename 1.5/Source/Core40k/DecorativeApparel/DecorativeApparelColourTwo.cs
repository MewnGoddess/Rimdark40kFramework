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

        public Dictionary<ExtraDecorationDef, bool> ExtraDecorationDefs => extraDecorations;

        public void AddOrRemoveDecoration(ExtraDecorationDef decoration)
        {
            if (extraDecorations.ContainsKey(decoration) && extraDecorations[decoration])
            {
                extraDecorations.Remove(decoration);
            }
            else if (extraDecorations.ContainsKey(decoration))
            {
                extraDecorations[decoration] = true;
            }
            else
            {
                extraDecorations.Add(decoration, false);
            }
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
            base.ExposeData();
        }
    }
}