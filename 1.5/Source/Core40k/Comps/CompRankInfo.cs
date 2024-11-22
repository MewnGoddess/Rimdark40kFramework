using System.Collections.Generic;
using Verse;


namespace Core40k
{
    public class CompRankInfo : ThingComp
    {
        public CompProperties_RankInfo Props => (CompProperties_RankInfo)props;
        
        public List<RankDef> unlockedRanks = new List<RankDef>();
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref unlockedRanks, "unlockedRanks");
        }
    }
}