using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MiliraWingsOfDemocracyFloatMenuFix
{
    [HarmonyPatch]
    internal static class SolarCrystalFloatMenuPatch
    {
        private const string SolarWorkGiverDefName = "PLAMilira_EmptySolarCrystalContainer";
        private const string DuplicateWorkGiverDefName = "PLAMilira_PoisonFood";
        private const string RearmWorkGiverDefName = "RearmPLAMiliraBuildings";
        private const string SolarTowerDefName = "Milira_SolarCrystalGatheringTower";
        private static readonly long CacheDurationTicks = Stopwatch.Frequency / 4;

        private static Pawn cachedPawn;
        private static Thing cachedTower;
        private static FloatMenuOption cachedOption;
        private static long cacheValidUntil;

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(FloatMenuOptionProvider_WorkGivers),
                "GetWorkGiverOption");
        }

        private static bool Prefix(
            Pawn pawn,
            WorkGiverDef workGiver,
            LocalTargetInfo target,
            ref FloatMenuOption __result,
            out bool __state)
        {
            __state = false;
            if (workGiver == null)
            {
                return true;
            }

            Thing clickedThing = target.Thing;

            // Both entries use the same refueling drivers for this building.
            // Keep Rearm while drafted, when Core's Refuel is unavailable.
            if (workGiver.defName == RearmWorkGiverDefName
                && pawn != null
                && !pawn.Drafted
                && clickedThing != null
                && clickedThing.def != null
                && clickedThing.def.defName == SolarTowerDefName)
            {
                __result = null;
                return false;
            }

            // In the original 1.6 defs this entry uses the same worker, labels,
            // and extraction job as the solar-container entry. It only duplicates
            // the same expensive float-menu check and option.
            if (workGiver.defName == DuplicateWorkGiverDefName)
            {
                __result = null;
                return false;
            }

            if (workGiver.defName != SolarWorkGiverDefName)
            {
                return true;
            }

            Thing tower = clickedThing;
            PLAMilira.CompGenerator_SolarCrystal generator =
                tower == null ? null : tower.TryGetComp<PLAMilira.CompGenerator_SolarCrystal>();

            if (tower == null
                || tower.def == null
                || tower.def.defName != SolarTowerDefName
                || generator == null)
            {
                __result = null;
                return false;
            }

            if (!tower.Spawned || !generator.CanEmptyNow)
            {
                Invalidate(pawn, tower);
                __result = null;
                return false;
            }

            long now = Stopwatch.GetTimestamp();
            if (ReferenceEquals(cachedPawn, pawn)
                && ReferenceEquals(cachedTower, tower)
                && now <= cacheValidUntil)
            {
                __result = cachedOption;
                return false;
            }

            __state = true;
            return true;
        }

        private static void Postfix(
            Pawn pawn,
            LocalTargetInfo target,
            ref FloatMenuOption __result,
            bool __state)
        {
            if (!__state)
            {
                return;
            }

            cachedPawn = pawn;
            cachedTower = target.Thing;
            cachedOption = __result;
            cacheValidUntil = Stopwatch.GetTimestamp() + CacheDurationTicks;
        }

        private static void Invalidate(Pawn pawn, Thing tower)
        {
            if (ReferenceEquals(cachedPawn, pawn) && ReferenceEquals(cachedTower, tower))
            {
                cachedOption = null;
                cacheValidUntil = 0;
            }
        }
    }
}
