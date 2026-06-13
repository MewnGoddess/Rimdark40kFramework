using HarmonyLib;
using RimWorld;
using Verse;

namespace Core40k;

[HarmonyPatch(typeof(SkillRecord), "Aptitude", MethodType.Getter)]
public class SkillRecordAptitudePatch
{
    public static void Postfix(Pawn ___pawn)
    {
        
    }
}