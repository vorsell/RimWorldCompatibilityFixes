using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoeLotlTradeFactionFix
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        internal const string HarmonyId = "local.moelotl.settlementslavefactionfix";

        static Startup()
        {
            new Harmony(HarmonyId).PatchAll();
            Log.Message("[MLTSFF] MoeLotl Settlement Slave Faction Fix initialized.");
        }
    }

    internal static class GenerationScope
    {
        internal const string TargetFactionDef = "AxolotlWanderingDynasty";
        internal const string TargetTraderKind = "Base_Axolotl_Stander";
        internal const string TargetPawnKind = "Axolotl_Slave";

        [ThreadStatic]
        private static int depth;

        [ThreadStatic]
        private static Faction settlementFaction;

        internal static bool Active
        {
            get { return depth > 0 && settlementFaction != null; }
        }

        internal static Faction SettlementFaction
        {
            get { return settlementFaction; }
        }

        internal static bool TryEnter(Settlement_TraderTracker tracker)
        {
            if (tracker == null
                || tracker.TraderKind == null
                || tracker.TraderKind.defName != TargetTraderKind)
            {
                return false;
            }

            Settlement settlement = AccessTools.FieldRefAccess<Settlement_TraderTracker, Settlement>(
                tracker,
                "settlement");

            Faction faction = settlement == null ? null : settlement.Faction;
            if (faction == null
                || faction.def == null
                || faction.def.defName != TargetFactionDef)
            {
                return false;
            }

            if (depth == 0)
            {
                settlementFaction = faction;
            }

            depth++;
            return true;
        }

        internal static void Exit()
        {
            if (depth > 0)
            {
                depth--;
            }

            if (depth == 0)
            {
                settlementFaction = null;
            }
        }
    }

    [HarmonyPatch(typeof(Settlement_TraderTracker), "RegenerateStock")]
    internal static class SettlementStockScopePatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix(Settlement_TraderTracker __instance, out bool __state)
        {
            __state = GenerationScope.TryEnter(__instance);
        }

        [HarmonyPostfix]
        private static void Postfix(bool __state)
        {
            if (__state)
            {
                GenerationScope.Exit();
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, bool __state)
        {
            if (__state && __exception != null)
            {
                GenerationScope.Exit();
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", typeof(PawnGenerationRequest))]
    internal static class ForceMoeLotlSlaveFactionPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)]
        [HarmonyBefore("raceQuestPawn")]
        private static void Prefix(ref PawnGenerationRequest request)
        {
            if (!GenerationScope.Active
                || request.KindDef == null
                || request.KindDef.defName != GenerationScope.TargetPawnKind)
            {
                return;
            }

            Faction faction = GenerationScope.SettlementFaction;
            if (request.Faction == faction)
            {
                return;
            }

            string oldFaction = request.Faction == null || request.Faction.def == null
                ? "null"
                : request.Faction.def.defName;

            request.Faction = faction;
            Log.Message(
                "[MLTSFF] Axolotl_Slave faction corrected: "
                + oldFaction
                + " -> "
                + faction.def.defName);
        }
    }
}
