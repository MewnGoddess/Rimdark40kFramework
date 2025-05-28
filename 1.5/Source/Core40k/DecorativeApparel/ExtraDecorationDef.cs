using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Core40k;

public class ExtraDecorationDef : DecorationDef
{
    public bool isHelmetDecoration = false;
        
    public List<Rot4> defaultShowRotation = new() {Rot4.North, Rot4.South, Rot4.East, Rot4.West};

    public ShaderTypeDef shaderType = null;

    public DrawData drawData = new();

    public bool colourable = false;
        
    public bool flipable = false;

    public bool hasArmorColourPaletteOption = false;
        
    public bool useArmorColourAsDefault = false;
        
    public List<string> appliesTo = new List<string>();
        
    public bool appliesToAll = false;

    public bool useMask = false;
        
    public bool drawInHeadSpace = false;
    
    public Vector2 drawSize = Vector2.one;
}