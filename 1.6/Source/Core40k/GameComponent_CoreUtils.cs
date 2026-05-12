using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Core40k;

public class GameComponent_CoreUtils : GameComponent
{
    private List<Pawn> pawnList;
    private List<CachedDecoratives> cachedDecorativesList;
    
    public Dictionary<Pawn, CachedDecoratives> cachedDecoratives = new ();
    
    public GameComponent_CoreUtils(Game game)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref cachedDecoratives, "cachedDecoratives", LookMode.Reference, LookMode.Deep, ref pawnList, ref cachedDecorativesList);
    }
    
    public class CachedDecoratives : IExposable
    {
        public List<Apparel> apparels = [];
        public ThingWithComps weapon;
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref apparels, "apparels", LookMode.Reference);
            Scribe_References.Look(ref weapon, "weapon");

        }
    }
}