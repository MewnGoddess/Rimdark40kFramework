using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class PawnRenderNodePropertiesMultiColor : PawnRenderNodeProperties
{
    public Color? colorTwo;
    public Color? colorThree;
    public MaskDef maskDef;
    public bool useMask;
    public BodyTypeDef bodyType;
    public bool useBodyType;
    public List<DecorationFlag> decorationFlags = new ();
}