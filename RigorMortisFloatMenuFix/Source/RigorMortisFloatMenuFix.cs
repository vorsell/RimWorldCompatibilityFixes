using HarmonyLib;
using Verse;

namespace RigorMortisFloatMenuFix
{
    [StaticConstructorOnStartup]
    public static class RigorMortisFloatMenuFixStartup
    {
        static RigorMortisFloatMenuFixStartup()
        {
            IncantationGraphicCache.Initialize();
            new Harmony("kaga.rigormortis.floatmenufix").PatchAll();
        }
    }
}
