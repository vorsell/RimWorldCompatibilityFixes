using HarmonyLib;
using Verse;

namespace MiliraWingsOfDemocracyFloatMenuFix
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("vorsel.milirawingsofdemocracy.floatmenufix").PatchAll();
        }
    }
}
