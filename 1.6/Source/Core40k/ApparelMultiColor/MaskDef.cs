using System.Collections.Generic;
using Verse;

namespace Core40k;

public class MaskDef : Def
{
    public string maskPath;
    public List<string> appliesTo = new List<string>();
    public int sortOrder = 1;
    public List<string> maskExtraFlags = new List<string>();
    public bool setsNull = false;
}