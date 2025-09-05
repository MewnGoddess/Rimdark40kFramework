using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class DecorativeApparelMultiColor : ApparelMultiColor
{
    private Dictionary<ExtraDecorationDef, ExtraDecorationSettings> originalExtraDecorations = new ();
    private Dictionary<ExtraDecorationDef, ExtraDecorationSettings> extraDecorations = new ();
    
    public Dictionary<ExtraDecorationDef, ExtraDecorationSettings> ExtraDecorations => extraDecorations;
    
    private bool movedToComp = false;
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref extraDecorations, "extraDecorations");
        Scribe_Collections.Look(ref originalExtraDecorations, "originalExtraDecorations");
        
        Scribe_Values.Look(ref movedToComp, "movedToComp");
        
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (!movedToComp && this.HasComp<CompDecorative>())
            {
                var decorative = GetComp<CompDecorative>();
                decorative.TempSetInitialValues(this);
                movedToComp = true;
            }
        }
    }
}