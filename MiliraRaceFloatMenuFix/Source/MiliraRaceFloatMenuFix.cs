using HarmonyLib;
using Verse;

namespace MiliraRaceFloatMenuFix
{
    [StaticConstructorOnStartup]
    public static class MiliraRaceFloatMenuFixStartup
    {
        static MiliraRaceFloatMenuFixStartup()
        {
            new Harmony("vorsel.milirarace.floatmenufix").PatchAll();
        }
    }
}
