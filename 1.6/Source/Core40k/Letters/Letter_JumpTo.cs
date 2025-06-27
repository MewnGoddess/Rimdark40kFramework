using System.Collections.Generic;
using Verse;

namespace Core40k;

public class Letter_JumpTo : ChoiceLetter
{
    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (lookTargets.IsValid())
            {
                yield return Option_JumpToLocation;
            }
            yield return Option_Close;
        }
    }
}