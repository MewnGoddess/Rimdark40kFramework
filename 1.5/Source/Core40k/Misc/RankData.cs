using System.Xml;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;


namespace Core40k
{
    public class RankData
    {
        public RankDef rankDef;

        public int daysAs = 0;

        public RankData()
        {
        }

        public RankData(RankDef rankDef, int daysAs)
        {
            this.rankDef = rankDef;
            this.daysAs = daysAs;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "rankDef", xmlRoot.Name, null, null, typeof(RankDef));
            if (xmlRoot.FirstChild != null)
            {
                daysAs = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
            }
        }
    }
}