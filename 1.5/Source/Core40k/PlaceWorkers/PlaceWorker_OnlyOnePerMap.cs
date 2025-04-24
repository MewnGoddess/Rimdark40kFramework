using Verse;

namespace Core40k;

public class PlaceWorker_OnlyOnePerMap : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        if (map.listerBuildings.ColonistsHaveBuilding((ThingDef)checkingDef))
        {
            return "BEWH.Framework.PlacementLimit.OnlyOneBuildingAllowedPerMap".Translate(((ThingDef)checkingDef).label.CapitalizeFirst());
        }
        return true;
    }
}