using System.Collections.Generic;
using Verse;


namespace Core40k
{
    public class GameComponent_RankInfo : GameComponent
    {
        //Counts rankDef limits, meaning that each time a rank is unlocked that is limited, it is counted here.
        public Dictionary<RankDef, int> rankLimits = new Dictionary<RankDef, int>();

        public GameComponent_RankInfo(Game game)
        {
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref rankLimits, "rankLimits");
        }
    }
}