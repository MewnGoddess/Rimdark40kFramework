using System.Collections.Generic;
using VanillaGenesExpanded;
using Verse;

namespace Genes40k
{
    public class ExtraDecorationDef : DecorationDef
    {
        public bool isHelmetDecoration = false;
        
        public List<Rot4> defaultShowRotation = new() {Rot4.North, Rot4.South, Rot4.East, Rot4.West};

        public ShaderTypeDef shaderType = null;
        
        public Dictionary<Rot4, float> layerOffsets = new Dictionary<Rot4, float>();

        public bool colourable = false;
        
        public bool flipable = false;

        public bool hasArmorColourPaletteOption = false;
        
        public bool useArmorColourAsDefault = false;
        
        public List<string> appliesTo = new List<string>();
        
        public bool appliesToAll = false;
    }
}   