using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Core40k;

public class GameComponent_CoreUtils : GameComponent
{
    public Dictionary<Pawn, CachedDecoratives> cachedDecoratives = new ();
    
    public Dictionary<Pawn, CachedDecoratives> cachedAlternateTexture = new ();

    public Dictionary<(Pawn, Thing), bool> cachedGizmoToggles = new();
    
    public GameComponent_CoreUtils(Game game)
    {
    }
    
    public class CachedDecoratives
    {
        public List<Apparel> apparels = [];
        public ThingWithComps weapon;
    }
}