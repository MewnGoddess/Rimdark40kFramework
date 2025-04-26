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

    public PresetData()
    {
    }

    public PresetData(ExtraDecorationDef extraDecorationDef, bool flipped, Color colour)
    {
        this.extraDecorationDef = extraDecorationDef;
        this.flipped = flipped;
        this.colour = colour;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "extraDecorationDef", xmlRoot.Name, null, null, typeof(ExtraDecorationDef));

        if (xmlRoot.FirstChild?.FirstChild != null)
        {
            flipped = ParseHelper.FromString<bool>(xmlRoot.FirstChild.FirstChild.Value);
        }
        if (xmlRoot.LastChild?.FirstChild != null)
        {
            colour = ParseHelper.FromString<Color>(xmlRoot.LastChild.FirstChild.Value);
        }
    }
}