using Verse;

namespace Core40k;

public class CompGraphicParent : ThingComp
{
    private bool initialSet;

    public bool InitialSet
    {
        get => initialSet;
        set => initialSet = value;
    }
    
    public virtual void Notify_GraphicChanged()
    {
        parent.Notify_ColorChanged();
    }
    
    public virtual void SetOriginals()
    {
    }

    public virtual void Reset()
    {
    }
    
    public virtual void InitialSetup()
    {
        SetOriginals();
        initialSet = true;
    }
    
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (initialSet)
        {
            return;
        }
        InitialSetup();
    }
    
    public override void PostExposeData()
    {
        Scribe_Values.Look(ref initialSet, "initialColourSet");
        base.PostExposeData();
    }
}