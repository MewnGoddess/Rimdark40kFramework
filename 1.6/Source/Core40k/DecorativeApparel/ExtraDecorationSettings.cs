using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationSettings : IExposable
{
    public Color Color = Color.white;
    public bool Flipped = false;
    
    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref Color, "Color");
        Scribe_Values.Look(ref Flipped, "Flipped");
    }
}