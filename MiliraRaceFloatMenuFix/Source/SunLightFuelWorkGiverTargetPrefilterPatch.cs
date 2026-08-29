using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MiliraRaceFloatMenuFix
{
    [HarmonyPatch]
    internal static class SunLightFuelWorkGiverTargetPrefilterPatch
    {
        private const string WorkGiverDefName = "Milira_EmptySunLightFuelContainer";
        private static readonly long CacheDurationTicks = Stopwatch.Frequency / 4;

        [ThreadStatic]
        private static bool evaluatingTower;

        private static Pawn cachedPawn;
        private static Thing cachedTower;
        private static FloatMenuOption cachedOption;
        private static long cacheValidUntil;

        internal static bool EvaluatingTower
        {
            get { return evaluatingTower; }
        }

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
            if (workGiver == null || workGiver.defName != WorkGiverDefName)
            {
                return true;
            }

            Thing tower = target.Thing;
            Milira.CompGenerator_SunLightFuel generator =
                tower == null ? null : tower.TryGetComp<Milira.CompGenerator_SunLightFuel>();

            // This WorkGiver can only act on a solar-fuel generator. Avoid its
            // global ShouldSkip scan for every unrelated thing and cell target.
            if (generator == null)
            {
                __result = null;
                return false;
            }

            // An empty or despawned tower cannot produce a direct-order option.
            if (!tower.Spawned || !generator.CanEmptyNow)
            {
                Invalidate(tower, pawn);
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

            evaluatingTower = true;
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

            evaluatingTower = false;
            cachedPawn = pawn;
            cachedTower = target.Thing;
            cachedOption = __result;
            cacheValidUntil = Stopwatch.GetTimestamp() + CacheDurationTicks;
        }

        private static Exception Finalizer(Exception __exception, bool __state)
        {
            if (__state)
            {
                evaluatingTower = false;
            }

            return __exception;
        }

        private static void Invalidate(Thing tower, Pawn pawn)
        {
            if (ReferenceEquals(cachedTower, tower) && ReferenceEquals(cachedPawn, pawn))
            {
                cacheValidUntil = 0;
                cachedOption = null;
            }
        }
    }

    [HarmonyPatch(typeof(Milira.WorkGiver_EmptySunLightFuelContainer), "ShouldSkip")]
    internal static class SunLightFuelWorkGiverFloatMenuShouldSkipPatch
    {
        private static bool Prefix(ref bool __result)
        {
            if (!SunLightFuelWorkGiverTargetPrefilterPatch.EvaluatingTower)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
