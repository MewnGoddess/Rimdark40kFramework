using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;

namespace Core40k;

public class PresetData
{
    public ExtraDecorationDef extraDecorationDef;

    public bool flipped = false;
    public Color? colour = null;
    public Color? colourTwo = null;
    public Color? colourThree = null;
    public MaskDef maskDef = null;

    public PresetData()
    {
    }

    public PresetData(ExtraDecorationDef extraDecorationDef, bool flipped, Color colour, Color colourTwo, Color colourThree, MaskDef maskDef)
    {
        this.extraDecorationDef = extraDecorationDef;
        this.flipped = flipped;
        this.colour = colour;
        this.colourTwo = colourTwo;
        this.colourThree = colourThree;
        this.maskDef = maskDef;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "extraDecorationDef", xmlRoot.Name, null, null, typeof(ExtraDecorationDef));
        foreach (var xmlNode in xmlRoot.ChildNodes)
        {
            var childNode = (XmlElement)xmlNode;

            switch (childNode.Name)
            {
                case "flipped":
                    flipped = ParseHelper.FromString<bool>(childNode.FirstChild.Value);
                    break;
                case "colour":
                    colour = ParseHelper.FromString<Color>(childNode.FirstChild.Value);
                    break;
                case "colourTwo":
                    colourTwo = ParseHelper.FromString<Color>(childNode.FirstChild.Value);
                    break;
                case "colourThree":
                    colourThree = ParseHelper.FromString<Color>(childNode.FirstChild.Value);
                    break;
                case "maskDef":
                    maskDef = ParseHelper.FromString<MaskDef>(childNode.FirstChild.Value);
                    break;
                default:
                    Log.Warning("Error in ExtraDecorationPresetDef, " + childNode.Name + " not recognized as a valid field.");
                    break;
            }
        }
    }
}